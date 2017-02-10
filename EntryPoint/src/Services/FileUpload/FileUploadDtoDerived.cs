using System;
using ServiceStack.ServiceHost;
using System.IO;

namespace EntryPoint
{
	[Route("/api/resources/data/{Id}", "PUT")]
	public class DataFileDto : FileDto
	{

	}

	[Route ("/api/resources/assemblies/{Id}", "PUT")]
	public class AssemblyFileDto : FileDto
	{
		
	}
}

