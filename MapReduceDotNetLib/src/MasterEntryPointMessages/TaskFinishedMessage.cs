using System;
using System.Collections.Generic;

namespace MapReduceDotNetLib
{
	public class TaskFinishedMessage
	{
		public int TaskId { get; set; }
		public List<Tuple<S3ObjectMetadata, List<string>>> reduceResult { get; set; }
	}
}
