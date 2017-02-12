using System;
namespace MapReduceDotNetLib
{
	public class AssemblyMetadata
	{
		public string Namespace { get; set; }
		public string MapClassName { get; set; }
		public string ReduceClassName { get; set; }
		public S3ObjectMetadata File { get; set; }
	}
}
