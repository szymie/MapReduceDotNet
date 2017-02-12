using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Worker
{
	public abstract class WorkerActor : TypedActor, IHandle<NewWorkerMessage>, IHandle<StopWorkerMessage>, IHandle<string>
	{
		protected WorkerConfig WorkerConfig{ get; set; }
		protected int TaskId{ get; set; }
		protected int CoordinatorId{ get; set; }
		protected int WorkerId{ get; set; }
		protected IActorRef Coordinator{ get; set;}
		protected LocalFilesDirectory LocalFileUtils{ get; set; }
		protected Thread WorkerThread{ get; set; }
		protected IActorRef self{ get; set; }

		protected Object uploadFilesLock{ get; set; } = new Object();

		public WorkerActor ()
		{
		}

		public void Handle (string message){
			Console.WriteLine (message);
		}

		public void Handle (NewWorkerMessage message){
			WorkerConfig = message.WorkerConfig;
			TaskId = WorkerConfig.WorkConfig.TaskId;
			CoordinatorId = WorkerConfig.CoordinatorId;
			WorkerId = WorkerConfig.WorkerId;
			Coordinator = Sender;
			self = Self;

			LocalFileUtils = new LocalFilesDirectory (WorkerConfig);

			WorkerThread = new Thread (() => {
				LocalFileUtils.createDirectory ();

				try{
					workProcessing();

					if(Monitor.TryEnter(uploadFilesLock)){
						uploadResult();
					}
				}
				catch(Exception e){
					Coordinator.Tell(new WorkerFailureMessage(WorkerId, TaskId, e), self);
				}
				finally{
					LocalFileUtils.removeDirectory();

					Monitor.Exit(uploadFilesLock);
				}
			});

			WorkerThread.Start ();
			Context.System.Scheduler.ScheduleTellRepeatedly (TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1), Self, "kill me", Self);
		}
		public void Handle (StopWorkerMessage message){
			if (Monitor.TryEnter (uploadFilesLock)) {
				this.WorkerThread.Abort ();

				this.LocalFileUtils.removeDirectory ();

				Monitor.Exit (uploadFilesLock);
			}
		}
		protected abstract void workProcessing ();

		protected abstract void uploadResult ();

		protected Work loadClientAssembly(S3ObjectMetadata file, string @namespace, string className){
			string asseblyFileName = LocalFileUtils.DirectoryPath + "assembly.dll";

			try{
				using (Stream stream = file.downStream ()) {
					using(FileStream fileStream = File.Create (asseblyFileName)){
						stream.CopyTo (fileStream);
					}
				}			

				Assembly assembly = Assembly.LoadFrom(asseblyFileName);
				Type type = assembly.GetType(@namespace + "." + className);
				Work work = (Work) Activator.CreateInstance(type);
				work.setEmitParams (LocalFileUtils.DirectoryPath);

				return work;
			}
			catch(Exception e){
				throw new NotImplementedException ();
			}
		}
	}
}

