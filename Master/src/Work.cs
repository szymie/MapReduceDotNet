using System;
using MapReduceDotNetLib;
using System.Collections.Generic;

namespace Master
{
	public class Work
	{
		public Work (int taskId, int workId, WorkConfig workConfig)
		{
			this.TaskId = taskId;
			this.WorkId = workId;
			this.WorkConfig = workConfig;
		}

		public int TaskId{get;set;}
		public int WorkId{get;set;}
		public WorkConfig WorkConfig{get;set;}
	}
}

