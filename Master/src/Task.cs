using System;
using System.Collections.Generic;
using Akka.Actor;
using MapReduceDotNetLib;

namespace Master
{
	public class Task
	{
		public int Id{get;set;}
		public Dictionary<IActorRef, Coordinator> MapCoordinators{ get; set; } = new Dictionary<IActorRef, Coordinator>();
		public Dictionary<IActorRef, Coordinator> ReduceCoordinators{ get; set; } = new Dictionary<IActorRef, Coordinator>();
		public Dictionary<string, List<S3ObjectMetadata>> MapResult{ get; set; } = new Dictionary<string, List<S3ObjectMetadata>>();
		public Dictionary<string, S3ObjectMetadata> ReduceResult{ get; set; } = new Dictionary<string, S3ObjectMetadata>();

		public Task (int id)
		{
			this.Id = id;
		}

		public bool getCoordinatorByActorRef(IActorRef coordinatorActor, out Coordinator coordinator){
			if (MapCoordinators.TryGetValue (coordinatorActor, out coordinator) || ReduceCoordinators.TryGetValue(coordinatorActor, out coordinator)) {
				return true;
			}

			return false;
		}
	}
}

