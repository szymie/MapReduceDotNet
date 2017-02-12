using System;
using System.Collections.Generic;
using Akka.Actor;
using MapReduceDotNetLib;

namespace Master
{
	public class Task
	{
		public Task (int id)
		{
			this.Id = id;
		}

		public int Id{get;set;}
		public Dictionary<IActorRef, Coordinator> mapCoordinators = new Dictionary<IActorRef, Coordinator>();
		public Dictionary<IActorRef, Coordinator> reduceCoordinators = new Dictionary<IActorRef, Coordinator>();
		public Dictionary<string, List<S3ObjectMetadata>> mapResult = new Dictionary<string, List<S3ObjectMetadata>>();
		public Dictionary<string, S3ObjectMetadata> reduceResult = new Dictionary<string, S3ObjectMetadata>();
	}
}

