using System;

namespace MapReduceDotNetLib
{
	public class WorkerFailureMessage
	{
		public WorkerFailureMessage (int workerId, int taskId, Exception exception)
		{
			this.WorkerId = workerId;
			this.TaskId = taskId;
			this.Exception = exception;
		}


		public int WorkerId{get;set;}
		public int TaskId{ get; set;}
		public Exception Exception{get;set;}
	}
}

