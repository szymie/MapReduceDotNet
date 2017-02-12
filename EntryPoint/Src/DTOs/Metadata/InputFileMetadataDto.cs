using System;
using ServiceStack.ServiceHost;

namespace EntryPoint
{
	public class MetadataDto
	{
		public int? Id { get; set; }
		public string Description { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	[Route("/api/resources/input-files", "GET,POST")]
	[Route("/api/resources/input-files/{Id}", "GET,DELETE")]
	public class InputFileMetadataDto : MetadataDto, IReturn<MetadataDtoResponse<InputFileMetadata>>
	{
		public string Name { get; set; }
	}
}
