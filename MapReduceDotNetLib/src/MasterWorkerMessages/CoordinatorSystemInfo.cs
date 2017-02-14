using System;

namespace MapReduceDotNetLib
{
	public class CoordinatorSystemInfo
	{
		public CoordinatorSystemInfo (float processPercent, int coordinatorId)
		{
			this.ProcessPercent = processPercent;
			this.CoordinatorId = coordinatorId;
		}

		public float ProcessPercent{ get; set;}
		public int CoordinatorId{get;set;}
	}
}

