using System;
using System.Collections.Generic;
using MapReduceDotNetLib;

namespace Master
{
	public class DivideRequestMessage
	{
		public DivideRequestMessage(int m, Dictionary<string, S3ObjectMetadata> files, int taskId)
		{
			this.M = m;
			this.Files = files;
			this.TaskId = taskId;
		}

		public int M {get;set;}
		public Dictionary<string, S3ObjectMetadata> Files {get;set;}
		public int TaskId {get;set;}
	}
}

