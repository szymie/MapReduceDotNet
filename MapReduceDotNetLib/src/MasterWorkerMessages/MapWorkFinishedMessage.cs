using System;
using System.Collections.Generic;

namespace MapReduceDotNetLib
{
	public class MapWorkFinishedMessage
	{
		public MapWorkFinishedMessage (int workerId, int taskId, Dictionary<string, S3ObjectMetadata> mapResult)
		{
			this.WorkerId = workerId;
			this.TaskId = taskId;
			this.MapResult = mapResult;
		}

		public int WorkerId{ get; set; }
		public int TaskId{ get; set; }
		public Dictionary<string, S3ObjectMetadata> MapResult{get;set;}
	}
}

