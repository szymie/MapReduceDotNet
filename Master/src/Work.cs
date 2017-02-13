using System;
using MapReduceDotNetLib;

namespace Master
{
	public class Work
	{
		public Work (int workId, WorkConfig workConfig)
		{
			this.WorkId = workId;
			this.WorkConfig = workConfig;
		}

		public int WorkId{get;set;}
		public WorkConfig WorkConfig{get;set;}
	}
}

