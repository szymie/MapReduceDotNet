using System;
using System.Collections.Generic;

namespace MapReduceDotNetLib
{
	public class TaskFinishedMessage
	{
		public int TaskId { get; set; }
		public Dictionary<string, S3ObjectMetadata> reduceResult;
	}
}
