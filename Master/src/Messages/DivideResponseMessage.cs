using System;
using System.Collections.Generic;
using MapReduceDotNetLib;
using OriginalFileName = System.String;

namespace Master
{
	public class DivideResponseMessage
	{
		public DivideResponseMessage(List<Dictionary<OriginalFileName, S3ObjectMetadata>> files, int taskId)
		{
			this.Files = files;
			this.TaskId = taskId;
		}

		public List<Dictionary<OriginalFileName, S3ObjectMetadata>> Files {get;set;}
		public int TaskId {get;set;}
	}
}

