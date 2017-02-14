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
		public bool IsMapCoordinator{ get; set; }

		private UniqueKeyGenerator orderedWorksKeyGenerator = new UniqueKeyGenerator ();

		public Coordinator (int id, IActorRef coordinatorActor, bool isMapCoordinator)
		{
			this.Id = id;
			this.CoordinatorActor = coordinatorActor;
			this.IsMapCoordinator = isMapCoordinator;
		}

		public int storeOrderedWork(WorkConfig workConfig){
			int orderedWorkId = orderedWorksKeyGenerator.generateKey ();
			OrderedWorks.Add (orderedWorkId, workConfig);

			return orderedWorkId;
		}

		public int countWorksForTask(int taskId){
			int counter = 0;
			foreach(Work work in Works.Values){
				if (work.TaskId == taskId) {
					counter++;
				}
			}

			return counter;
		}

		public void abort (int taskId)
		{
			foreach(Work work in Works.Values){
				if (work.TaskId == taskId) {
					CoordinatorActor.Tell (new AbortWorkMessage (work.WorkId));
				}
			}
		}
	}
}

