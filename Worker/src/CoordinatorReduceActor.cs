using System;
using Akka.Actor;
using MapReduceDotNetLib;

namespace Worker
{
	public class CoordinatorReduceActor : CoordinatorActor, IHandle<ReduceWorkFinishedMessage>
	{
		public CoordinatorReduceActor ()
		{
			MasterActor.Tell (new RegisterReduceCoordinatorMessage());
		}

		public void Handle (ReduceWorkFinishedMessage message){
			if (workers.ContainsKey (Sender)) {
				workers.Remove (Sender);
				MasterActor.Tell (message);


			} else {
				message.File.remove ();
			}

			Context.Stop (Sender);
		}

		protected override IActorRef createWorkerActor (int workerId)
		{
			return Context.System.ActorOf<ReduceActor> ("reduceWorker" + workerId);
		}
	}
}

