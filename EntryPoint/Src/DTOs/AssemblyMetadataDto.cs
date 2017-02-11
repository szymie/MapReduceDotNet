using System;
using ServiceStack.ServiceHost;

namespace EntryPoint
{
	[Route("/api/resources/assemblies", "GET,POST")]
	public class AssemblyMetadataDto : MetadataDto
	{
		public string Namespace { get; set; }
		public string MapClassName { get; set; }
		public string ReduceClassName { get; set; }
	}
}
