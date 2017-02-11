using System;

namespace EntryPoint
{
	public class AssemblyMetadata : InputFileMetadata
	{
		public string Namespace { get; set; }
		public string MapClassName { get; set; }
		public string ReduceClassName { get; set; }
	}
}
