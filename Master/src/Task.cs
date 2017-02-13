using System;
using System.Collections.Generic;
using Akka.Actor;
using MapReduceDotNetLib;

namespace Master
{
	public class Task
	{
		public int Id{get;set;}
		public int M{ get; set; }
		public int R{ get; set; }
		public List<S3ObjectMetadata> InputFiles{ get; set; }
		public AssemblyMetadata AssemblyMetadata{ get; set; }
		public string Username{ get; set; }
		public IActorRef SS{ get; set; }


		public Dictionary<IActorRef, Coordinator> MapCoordinators{ get; set; } = new Dictionary<IActorRef, Coordinator>();
		public Dictionary<IActorRef, Coordinator> ReduceCoordinators{ get; set; } = new Dictionary<IActorRef, Coordinator>();
		public Dictionary<string, List<S3ObjectMetadata>> MapResult{ get; set; } = new Dictionary<string, List<S3ObjectMetadata>>();
		public List<Tuple<S3ObjectMetadata, List<string>>> ReduceResult{ get; set; } = new List<Tuple<S3ObjectMetadata, List<string>>>(); //gdy widzisz liste< par <obiektow i list>>, to wiedz, ze cos sie dzieje

		public Task (NewTaskMessage newTaskMessage, IActorRef ss)
		{
			this.Id = newTaskMessage.TaskId;
			this.M = newTaskMessage.M;
			this.R = newTaskMessage.R;
			this.InputFiles = newTaskMessage.InputFiles;
			this.AssemblyMetadata = newTaskMessage.Assembly;
			this.Username = newTaskMessage.Username;
			this.SS = ss;
		}

		public bool getCoordinatorByActorRef(IActorRef coordinatorActor, out Coordinator coordinator, out bool isMapCoordinator){
			if (MapCoordinators.TryGetValue (coordinatorActor, out coordinator)){
				isMapCoordinator = true;
				return true;
			}

			if (ReduceCoordinators.TryGetValue (coordinatorActor, out coordinator)) {
				isMapCoordinator = false;
				return true;
			}

			isMapCoordinator = false;
			return false;
		}
	}
}

