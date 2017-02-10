using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;

namespace Worker
{
	public abstract class CoordinatorActor : TypedActor, IHandle<NewWorkMessage>
	{
		private Dictionary<int, IActorRef> workers;
		private int currentWorkerId = 0;

		public CoordinatorActor ()
		{
			ActorSelection masterActorRef = getMasterActorRef ();

			masterActorRef.Tell (new RegisterMapWorkerMessage ());
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
			int workerId = getUniqueWorkerId ();
			IActorRef workerActor = createWorkerActor(message.WorkerConfig, workerId);


			workers.Add (workerId, workerActor);
			Context.Watch (workerActor);
			Sender.Tell (new NewWorkAckMessage(workerId, message.WorkConfigId));
		}

		int getUniqueWorkerId ()
		{
			currentWorkerId++;

			currentWorkerId %= Int32.MaxValue;

			return currentWorkerId;
		}

		public void Handle (Terminated message)
		{			
			string disconnectedActorPath = message.ActorRef.Path.ToString();
			throw new NotImplementedException ();
		}

		protected abstract IActorRef createWorkerActor(WorkerConfig config, int workerId);
	}
}

