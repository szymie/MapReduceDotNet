using System;
using System.Collections.Generic;
using MapReduceDotNetLib;

namespace MapReduceDotNetLib
{
	public class NewTaskMessage
	{
		public NewTaskMessage()
		{
		}

		public NewTaskMessage(Dictionary<string, S3ObjectMetadata> inputFiles, AssemblyMetadata assembly, int m, int r, int taskId, string username)
		{
			this.InputFiles = inputFiles;
			this.Assembly = assembly;
			this.M = m;
			this.R = r;
			this.TaskId = taskId;
			this.Username = username;
		}

		public Dictionary<string, S3ObjectMetadata> InputFiles { get; set; }
		public AssemblyMetadata Assembly { get; set; }
		public int M { get; set; }
		public int R { get; set; }
		public int TaskId { get; set; }
		public string Username { get; set; }
	}
}
