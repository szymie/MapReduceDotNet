using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.Threading;

namespace Worker
{
	public abstract class CoordinatorActor : TypedActor, IHandle<NewWorkMessage>, IHandle<RegisterCoordinatorAckMessage>, IHandle<WorkerFailureMessage>, IHandle<AbortWorkMessage>
	{
		protected Dictionary<IActorRef, WorkerConfig> workers = new Dictionary<IActorRef, WorkerConfig>();
		private UniqueKeyGenerator keyGenerator = new UniqueKeyGenerator();
		private int CoordinatorId{get;set;}
		protected ActorSelection MasterActor{ get; set; }
		public CoordinatorActor ()
		{
			MasterActor = getMasterActorRef ();
		}

		ActorSelection getMasterActorRef ()
		{
			string masterAddress = Environment.GetEnvironmentVariable ("MASTER_ADDRESS");
			if (masterAddress == null) {
				masterAddress = "localhost:8081";
				Console.WriteLine ("No MASTER_ADDRESS found.");
			}

			Console.WriteLine ("MASTER_ADDRESS {0}", masterAddress);

			return Context.ActorSelection("akka.tcp://MasterSystem@" + masterAddress + "/user/MasterActor");
		}

		public void Handle (NewWorkMessage message)
		{
			int workerId = keyGenerator.generateKey();



			WorkerConfig workerConfig = new WorkerConfig (workerId, message.WorkConfig, CoordinatorId);

			startNewWorker (workerConfig);

			MasterActor.Tell (new NewWorkAckMessage(workerId, message.WorkConfigId));

		}

		private void startNewWorker(WorkerConfig workerConfig){
			IActorRef workerActor = createWorkerActor(workerConfig.WorkerId);

			workers.Add (workerActor, workerConfig);
			Context.Watch (workerActor);
			workerActor.Tell(new NewWorkerMessage(workerConfig));
		}

		public void Handle (RegisterCoordinatorAckMessage message)
		{
			this.CoordinatorId = message.CoordinatorId;
		}

		public void Handle (WorkerFailureMessage message){
			MasterActor.Tell (message);

			workers.Remove (Sender);
			Context.Stop (Sender);
		}

		public void Handle(AbortWorkMessage message){
			foreach (KeyValuePair<IActorRef, WorkerConfig> pair in workers) {
				if (pair.Value.WorkerId == message.WorkerId) {
					IActorRef worker = pair.Key;

					workers.Remove (worker);

					worker.Tell (new StopWorkerMessage());
					Context.Stop (worker);

					return;
				}
			}
		}

		public void Handle (Terminated message)
		{			
			WorkerConfig workerConfig;
			if (workers.TryGetValue (message.ActorRef, out workerConfig)) {
				LocalFilesDirectory dir = new LocalFilesDirectory (workerConfig);
				dir.removeDirectory ();

				Console.WriteLine ("Restarting worker...");

				startNewWorker (workerConfig);
			}
		}

		protected abstract IActorRef createWorkerActor(int workerId);
	}
}

