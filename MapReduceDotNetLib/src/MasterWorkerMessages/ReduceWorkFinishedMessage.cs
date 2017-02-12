using System;

namespace MapReduceDotNetLib
{
	public class ReduceWorkFinishedMessage
	{
		public ReduceWorkFinishedMessage (int workerId, int taskId, string key, S3ObjectMetadata file)
		{
			this.WorkerId = workerId;
			this.TaskId = taskId;
			this.Key = key;
			this.File = file;
		}
		
		public int WorkerId{ get; set; }
		public int TaskId{ get; set; }
		public string Key{ get; set;}
		public S3ObjectMetadata File{get;set;}
	}
}

