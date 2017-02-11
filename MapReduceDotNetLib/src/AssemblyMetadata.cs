using System;
namespace MapReduceDotNetLib
{
	public class AssemblyMetadata
	{
		public AssemblyMetadata (string @namespace, string mapClassName, string reduceClassName, S3ObjectMetadata file)
		{
			this.Namespace = @namespace;
			this.MapClassName = mapClassName;
			this.ReduceClassName = reduceClassName;
			this.File = file;
		}
		
		public string Namespace{ get; set; }
		public string MapClassName{ get; set; }
		public string ReduceClassName{ get; set; }
		public S3ObjectMetadata File{ get; set; }
	}
}