using System;
using ServiceStack.ServiceHost;

namespace EntryPoint
{
	[Route("/api/resources/assemblies/{Id}", "PUT")]
	public class AssemblyContentDto : ContentDto
	{
	}
}
