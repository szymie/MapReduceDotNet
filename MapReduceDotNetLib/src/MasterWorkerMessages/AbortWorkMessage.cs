using System;

namespace MapReduceDotNetLib
{
	public class AbortWorkMessage
	{
		public AbortWorkMessage (int workerId)
		{
			this.WorkerId = workerId;
		}
		

		public int WorkerId{get;set;}
	}
}

