using System;
using MapReduceDotNetLib;

namespace Worker
{
	public class WorkerConfig
	{
		public WorkerConfig (int workerId, WorkConfig workConfig, int coordinatorId)
		{
			this.WorkerId = workerId;
			this.WorkConfig = workConfig;
			this.CoordinatorId = coordinatorId;
		}

		public int WorkerId{ get; set;}
		public WorkConfig WorkConfig{get;set;}
		public int CoordinatorId{get;set;}
	}
}

