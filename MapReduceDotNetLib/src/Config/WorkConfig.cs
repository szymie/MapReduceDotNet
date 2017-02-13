using System;
using System.Collections.Generic;

namespace MapReduceDotNetLib
{
	public class WorkConfig
	{
		public WorkConfig (int taskId, string username, Dictionary<string, S3ObjectMetadata> filesToProcess, AssemblyMetadata assemblyMetaData)
		{
			this.TaskId = taskId;
			this.Username = username;
			this.FilesToProcess = filesToProcess;
			this.AssemblyMetaData = assemblyMetaData;
			this.ReduceKey = "";
		}

		public WorkConfig (int taskId, string username, Dictionary<string, S3ObjectMetadata> filesToProcess, AssemblyMetadata assemblyMetaData, string reduceKey)
		{
			this.TaskId = taskId;
			this.Username = username;
			this.FilesToProcess = filesToProcess;
			this.AssemblyMetaData = assemblyMetaData;
			this.ReduceKey = reduceKey;
		}
				
		public int TaskId{ get; set;}
		public string Username{ get; set;}
		public Dictionary<String, S3ObjectMetadata> FilesToProcess{ get; set;}
		public AssemblyMetadata AssemblyMetaData{ get; set;}
		public string ReduceKey{ get; set; }
	}
}

