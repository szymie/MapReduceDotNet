using System;
using System.Collections.Generic;

namespace MapReduceDotNetLib
{
	public class NewTaskMessage
	{
		public NewTaskMessage (List<S3ObjectMetadata> files, AssemblyMetadata assembly, int m, int r, int taskId, string username)
		{
			this.Files = files;
			this.Assembly = assembly;
			this.M = m;
			this.R = r;
			this.TaskId = taskId;
			this.Username = username;
		}

		public List<S3ObjectMetadata> Files{ get; set; }
		public AssemblyMetadata Assembly{get;set;}
		public int M{ get; set; }
		public int R{ get; set; }
		public int TaskId{get;set;}
		public string Username{get;set;}
	}
}

