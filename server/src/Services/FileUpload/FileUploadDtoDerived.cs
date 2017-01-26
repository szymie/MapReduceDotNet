using System;
using ServiceStack.ServiceHost;
using System.IO;

namespace Server
{
	[Route ("/resources/configs/{Id}", "POST")]
	public class ConfigFileUploadDto : FileUploadDto{}

	[Route ("/resources/assemblies/{Id}", "POST")]
	public class AssemblyFileUploadDto : FileUploadDto{}

	[Route ("/resources/datas/{Id}", "POST")]
	public class DataFileUploadDto : FileUploadDto{}
}

