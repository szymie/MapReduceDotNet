using System;
using Akka.Actor;
using MapReduceDotNetLib;

namespace Worker
{
	public class CoordinatorMapActor : CoordinatorActor, IHandle<MapWorkFinishedMessage>
	{
		public void Handle (MapWorkFinishedMessage message){
			MasterActor.Tell (message);
		}

		protected override IActorRef createWorkerActor (WorkConfig config, int workerId)
		{
			return Context.System.ActorOf<MapActor> ("mapWorker" + workerId);
		}
	}
}

