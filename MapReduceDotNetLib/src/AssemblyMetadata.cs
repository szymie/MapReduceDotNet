using System;

namespace MapReduceDotNetLib
{
	public class AssemblyMetadata
	{
		public string Filename { get; set; }
		public string ClassNamespace { get; set; }
		public string MapClassName { get; set; }
		public string ReduceClassName { get; set; }

		public AssemblyMetadata(string filename, string classNamespace, string mapClassName, string reduceClassName)
		{
			Filename = filename;
			ClassNamespace = classNamespace;
			MapClassName = mapClassName;
			ReduceClassName = reduceClassName;
		}
	}
}
