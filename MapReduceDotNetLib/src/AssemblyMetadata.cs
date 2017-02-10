using System;
namespace MapReduceDotNetLib
{
	public class AssemblyMetadata
	{
		public AssemblyMetadata (string @namespace, string mapClassName, string reduceClassName, string filename)
		{
			this.Namespace = @namespace;
			this.MapClassName = mapClassName;
			this.ReduceClassName = reduceClassName;
			this.Filename = filename;
		}
		
		public string Namespace{ get; set; }
		public string MapClassName{ get; set; }
		public string ReduceClassName{ get; set; }
		public string Filename{ get; set; }
	}
}