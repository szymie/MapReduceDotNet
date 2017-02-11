using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Worker
{
	public abstract class WorkerActor : TypedActor, IHandle<NewWorkerMessage>
	{
		protected WorkerConfig WorkerConfig{ get; set; }
		protected int TaskId{ get; set; }
		protected int CoordinatorId{ get; set; }
		protected int WorkerId{ get; set; }
		protected IActorRef Coordinator{ get; set;}
		protected LocalFilesDirectory LocalFileUtils{ get; set; }
		protected Thread WorkThread{ get; set; }

		public WorkerActor ()
		{
		}

		public void Handle (NewWorkerMessage message){
			WorkerConfig = message.WorkerConfig;
			TaskId = WorkerConfig.WorkConfig.TaskId;
			CoordinatorId = WorkerConfig.CoordinatorId;
			WorkerId = WorkerConfig.WorkerId;
			Coordinator = Sender;
			LocalFileUtils = new LocalFilesDirectory (WorkerConfig);

			WorkThread = new Thread (() => {
				LocalFileUtils.createDirectory ();

				try{
					workProcessing();
				}
				catch(Exception e){
					Coordinator.Tell(new WorkerFailureMessage(WorkerId, TaskId, e));
				}
				finally{
					LocalFileUtils.removeDirectory();
				}
			});

			WorkThread.Start ();
			
		}

		protected abstract void workProcessing ();

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

