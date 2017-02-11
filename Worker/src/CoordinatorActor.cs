using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;

namespace Worker
{
	public abstract class CoordinatorActor : TypedActor, IHandle<NewWorkMessage>, IHandle<RegisterCoordinatorAckMessage>, IHandle<WorkerFailureMessage>
	{
		private Dictionary<int, IActorRef> workers = new Dictionary<int, IActorRef>();
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
			IActorRef workerActor = createWorkerActor(message.WorkConfig, workerId);

			workers.Add (workerId, workerActor);
			Context.Watch (workerActor);
			MasterActor.Tell (new NewWorkAckMessage(workerId, message.WorkConfigId));
			workerActor.Tell(new NewWorkerMessage(new WorkerConfig(workerId, message.WorkConfig, CoordinatorId)));
		}

		public void Handle (RegisterCoordinatorAckMessage message)
		{
			this.CoordinatorId = message.CoordinatorId;
		}

		public void Handle (WorkerFailureMessage message){
			MasterActor.Tell (message);
			throw new NotImplementedException ();
		}

		public void Handle (Terminated message)
		{			
			string disconnectedActorPath = message.ActorRef.Path.ToString();
			throw new NotImplementedException ();
		}

		protected abstract IActorRef createWorkerActor(WorkConfig config, int workerId);
	}
}

