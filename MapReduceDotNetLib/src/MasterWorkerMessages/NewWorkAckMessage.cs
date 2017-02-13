using System;

namespace MapReduceDotNetLib
{
	public class NewWorkAckMessage
	{
		public NewWorkAckMessage (int workerId, int workId, int taskId, int coordinatorId)
		{
			this.WorkerId = workerId;
			this.OrderedWorkId = workId;
			this.TaskId = taskId;
			this.CoordinatorId = coordinatorId;
		}
		
		public int WorkerId{ get; set;}
		public int OrderedWorkId { get; set;}
		public int TaskId { get; set;}
		public int CoordinatorId { get; set;}
	}
}

