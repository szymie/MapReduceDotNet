using System;

namespace MapReduceDotNetLib
{
	public class NewWorkAckMessage
	{
		public NewWorkAckMessage (int workerId, int workId)
		{
			this.WorkerId = workerId;
			this.WorkId = workId;
		}
		
		public int WorkerId{ get; set;}
		public int WorkId { get; set;}
	}
}

