using System;

namespace MapReduceDotNetLib
{
	public class WorkerFailureMessage
	{
		
		public WorkerFailureMessage (int workerId, int taskId, String message)
		{
			this.WorkerId = workerId;
			this.TaskId = taskId;
			this.Message = message;
		}


		public int WorkerId{get;set;}
		public int TaskId{ get; set;}
		public string Message{get;set;}
	}
}

