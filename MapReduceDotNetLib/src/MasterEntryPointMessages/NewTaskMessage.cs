using System;
using System.Collections.Generic;
using MapReduceDotNetLib;

namespace MapReduceDotNetLib
{
	public class NewTaskMessage
	{
		public List<S3ObjectMetadata> InputFiles { get; set; }
		public AssemblyMetadata Assembly { get; set; }
		public int M { get; set; }
		public int R { get; set; }
		public int TaskId { get; set; }
		public string Username { get; set; }
	}
}
