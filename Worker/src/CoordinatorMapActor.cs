using System;
using Akka.Actor;
using MapReduceDotNetLib;

namespace Worker
{
	public class CoordinatorMapActor : CoordinatorActor
	{
		protected override IActorRef createWorkerActor (WorkerConfig config, int workerId)
		{
			return Context.System.ActorOf<MapActor> ("mapWorker" + workerId);
		}
	}
}

