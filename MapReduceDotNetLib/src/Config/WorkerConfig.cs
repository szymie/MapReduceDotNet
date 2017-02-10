using System;
using System.Collections.Generic;

namespace MapReduceDotNetLib
{
	public class WorkerConfig
	{
		public int TaskId{ get; set;}
		public string Username{ get; set;}
		public Dictionary<String, S3ObjectMetadata> FilesToProcess{ get; set;}
		public AssemblyMetadata AssemblyMetaData{ get; set;}
	}
}

