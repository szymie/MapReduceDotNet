using System;

namespace MapReduceDotNetLib
{
	public abstract class Work
	{
		protected int TaskId{ get; set; }
		protected int CoordinatorId{ get; set; }
		protected int WorkerId{ get; set; }
		protected UniqueKeyGenerator KeyGenerator = new UniqueKeyGenerator();

		public void setEmitParams (int taskId, int coordinatorId, int workerId)
		{
			this.TaskId = taskId;
			this.CoordinatorId = coordinatorId;
			this.WorkerId = workerId;
		}
	}
}

