using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Worker
{
	public abstract class WorkerActor : TypedActor, IHandle<NewWorkerMessage>, IHandle<StopWorkerMessage>
	{
		protected WorkerConfig WorkerConfig{ get; set; }
		protected int TaskId{ get; set; }
		protected int CoordinatorId{ get; set; }
		protected int WorkerId{ get; set; }
		protected IActorRef Coordinator{ get; set;}
		protected LocalFilesDirectory LocalFileUtils{ get; set; }
		protected Thread WorkerThread{ get; set; }
		protected IActorRef self{ get; set; }
		protected AssemblyMetadata AssemblyMetaData{ get; set; }
		protected Object uploadFilesLock{ get; set; } = new Object();
		protected string ReduceKey{ get; set; }

		public WorkerActor ()
		{
		}

		public void Handle (NewWorkerMessage message){
			WorkerConfig = message.WorkerConfig;
			TaskId = WorkerConfig.WorkConfig.TaskId;
			CoordinatorId = WorkerConfig.CoordinatorId;
			WorkerId = WorkerConfig.WorkerId;
			Coordinator = Sender;
			self = Self;
			AssemblyMetaData = WorkerConfig.WorkConfig.AssemblyMetaData;

			LocalFileUtils = new LocalFilesDirectory (WorkerConfig);

			WorkerThread = new Thread (() => {			
				try{
					LocalFileUtils.createDirectory ();

					workProcessing();

					if(Monitor.TryEnter(uploadFilesLock)){
						uploadResult();
					}
				}
				catch(ThreadAbortException e){

				}
				catch(Exception e){
					Coordinator.Tell(new WorkerFailureMessage(WorkerId, TaskId, e.Message), self);
				}
				finally{					
					LocalFileUtils.removeDirectory();

					Monitor.Exit(uploadFilesLock);
				}
			});

			WorkerThread.Start ();
		}
		public void Handle (StopWorkerMessage message){
			if (Monitor.TryEnter (uploadFilesLock)) {
				this.WorkerThread.Abort ();

				//this.LocalFileUtils.removeDirectory ();

				Monitor.Exit (uploadFilesLock);
			}
		}
		protected abstract void workProcessing ();

		protected abstract void uploadResult ();

		protected override void PostStop(){
			if (Monitor.TryEnter (uploadFilesLock)) {
				this.WorkerThread.Abort ();
				Monitor.Exit (uploadFilesLock);
			}
		}

		protected Work loadClientAssembly(string className){
			S3ObjectMetadata file = AssemblyMetaData.File;
			string @namespace = AssemblyMetaData.Namespace;

			string asseblyFileName = LocalFileUtils.DirectoryPath + "assembly.dll";

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
	}
}

