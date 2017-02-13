using System;
using System.Collections.Generic;

namespace MapReduceDotNetLib
{
	public class ReduceWorkFinishedMessage
	{
		public ReduceWorkFinishedMessage (int workerId, int taskId, List<string> keys, S3ObjectMetadata file)
		{
			this.WorkerId = workerId;
			this.TaskId = taskId;
			this.Keys = keys;
			this.File = file;
		}
		
		public int WorkerId{ get; set; }
		public int TaskId{ get; set; }
		public List<string> Keys{ get; set;}
		public S3ObjectMetadata File{get;set;}
	}
}

