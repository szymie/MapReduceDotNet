using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Threading;

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
			Console.WriteLine ("Creating new reduce actor: " + this.CoordinatorId);
			Thread.Sleep (2000);
			return Context.System.ActorOf<ReduceActor> ("reduceWorker" + workerId);
		}
	}
}

