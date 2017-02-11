using System;
using ServiceStack.ServiceHost;

namespace EntryPoint
{
	[Route("/api/resources/assemblies", "GET,POST")]
	[Route("/api/resources/assemblies/{Id}", "GET,PUT,DELETE")]
	public class AssemblyMetadataDto : MetadataDto, IReturn<MetadataDtoResponse<AssemblyMetadata>>
	{
		public string Namespace { get; set; }
		public string MapClassName { get; set; }
		public string ReduceClassName { get; set; }
	}
}
