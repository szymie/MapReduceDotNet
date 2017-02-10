using System;
using MapReduceDotNetLib;

namespace Worker
{
	public class WorkerConfig
	{
		public WorkerConfig (int workerId, WorkConfig workConfig)
		{
			this.WorkerId = workerId;
			this.WorkConfig = workConfig;
		}
		
		public int WorkerId{ get; set;}
		public WorkConfig WorkConfig{get;set;}
	}
}

