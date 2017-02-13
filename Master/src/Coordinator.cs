using System;
using System.Collections.Generic;
using MapReduceDotNetLib;
using Akka.Actor;

namespace Master
{
	public class Coordinator
	{
		public int Id{ get; set;}
		public Dictionary<int, Work> Works{ get; set;} = new Dictionary<int, Work>();
		public Dictionary<int, WorkConfig> OrderedWorks{ get; set;} = new Dictionary<int, WorkConfig>();
		public IActorRef CoordinatorActor{ get; set; }

		private UniqueKeyGenerator orderedWorksKeyGenerator = new UniqueKeyGenerator ();

		public Coordinator (int id, IActorRef coordinatorActor)
		{
			this.Id = id;
			this.CoordinatorActor = coordinatorActor;
		}

		public int storeOrderedWork(WorkConfig workConfig){
			int orderedWorkId = orderedWorksKeyGenerator.generateKey ();
			OrderedWorks.Add (orderedWorkId, workConfig);

			return orderedWorkId;
		}
	}
}

