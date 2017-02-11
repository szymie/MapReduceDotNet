using System;
using System.IO;
using ServiceStack.ServiceHost;

namespace EntryPoint
{
	public class ContentDto : IRequiresRequestStream
	{
		public int? Id { get; set; }
		public Stream RequestStream { get; set; }
	}

	[Route("/api/resources/input-files/{Id}", "PUT")]
	public class InputFileContentDto : ContentDto
	{
	}
}
