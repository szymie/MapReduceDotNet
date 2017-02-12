using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.IO;

namespace Master
{
	public class MasterActor : TypedActor, IHandle<RegisterMapCoordinatorMessage>, IHandle<NewWorkAckMessage>, IHandle<Terminated>, IHandle<MapWorkFinishedMessage>, IHandle<WorkerFailureMessage>,
	IHandle<RegisterReduceCoordinatorMessage>, IHandle<NewTaskMessage>
	{
		private Dictionary<int, IActorRef> validMapCoordinator = new Dictionary<int, IActorRef>();
		private Dictionary<int, IActorRef> validReduceCoordinator = new Dictionary<int, IActorRef>();

		private UniqueKeyGenerator coordinatorKeyGenerator = new UniqueKeyGenerator();

		public void Handle (NewTaskMessage message){
			throw new NotImplementedException ();
		}

		public void Handle (RegisterMapCoordinatorMessage message){
			int coordinatorId = coordinatorKeyGenerator.generateKey ();
			validMapCoordinator.Add (coordinatorId, Sender);
			Sender.Tell (new RegisterCoordinatorAckMessage(coordinatorId));
		}

		public void Handle (RegisterReduceCoordinatorMessage message){
			int coordinatorId = coordinatorKeyGenerator.generateKey ();
			validReduceCoordinator.Add (coordinatorId, Sender);
			Sender.Tell (new RegisterCoordinatorAckMessage(coordinatorId));
		}

		public void Handle (NewWorkAckMessage message){
			throw new NotImplementedException ();
		}

		public void Handle (WorkerFailureMessage message)
		{
			throw new NotImplementedException ();
		}

		public void Handle (MapWorkFinishedMessage message){
			throw new NotImplementedException ();
		}

		public void Handle (ReduceWorkFinishedMessage message){
			throw new NotImplementedException ();
		}



		public void Handle (Terminated message)
		{
			throw new NotImplementedException ();
		}
			
		public MasterActor ()
		{			

		}
	}
}

