using System;

namespace MapReduceDotNetLib
{
	public class TaskFailureMessage
	{
		public int TaskId { get; set; }
		public string Username { get; set; }
		public string Message { get; set; }

		public TaskFailureMessage (){
			
		}

		public TaskFailureMessage (int taskId, string message)
		{
			this.TaskId = taskId;
			this.Message = message;
		}
	}
}
