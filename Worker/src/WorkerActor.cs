using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.IO;
using System.Reflection;

namespace Worker
{
	public abstract class WorkerActor : TypedActor, IHandle<NewWorkerMessage>
	{
		protected WorkerConfig WorkerConfig{ get; set; }
		protected int TaskId{ get; set; }
		protected int CoordinatorId{ get; set; }
		protected int WorkerId{ get; set; }
		protected IActorRef Coordinator{ get; set;}
		public WorkerActor ()
		{
		}

		public void Handle (NewWorkerMessage message){
			WorkerConfig = message.WorkerConfig;
			TaskId = WorkerConfig.WorkConfig.TaskId;
			CoordinatorId = WorkerConfig.CoordinatorId;
			WorkerId = WorkerConfig.WorkerId;
			Coordinator = Sender;
		}

		protected Work loadClientAssembly(S3ObjectMetadata file, string @namespace, string className){
			string asseblyFileName = String.Format (LocalFileIO.localFilesLocation + "{0}-{1}-{2}-assembly.dll", TaskId, CoordinatorId, WorkerId);

			try{
				using (Stream stream = file.downStream ()) {
					using(FileStream fileStream = File.Create (asseblyFileName)){
						stream.CopyTo (fileStream);
					}
				}			

				Assembly assembly = Assembly.LoadFrom(asseblyFileName);
				Type type = assembly.GetType(@namespace + "." + className);
				Work work = (Work) Activator.CreateInstance(type);
				work.setEmitParams (TaskId, CoordinatorId, WorkerId);

				return work;
			}
			catch(Exception e){
				throw new NotImplementedException ();
			}
		}
	}
}

