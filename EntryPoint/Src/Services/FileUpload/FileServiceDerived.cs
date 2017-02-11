using System;
using ServiceStack.Common.Web;
using System.Net;
using MapReduceDotNetLib;
using ServiceStack.ServiceInterface;
using System.IO;

namespace EntryPoint
{
	public class AssemblyFileService : Service
	{
		public object Put(DataFileDto request)
		{
			var file = new S3ObjectMetadata("map-reduce-dot-net", "file1");

			var memoryStream = new MemoryStream();
			request.RequestStream.CopyTo(memoryStream);

			file.upStream(memoryStream);

			return new HttpResult(HttpStatusCode.OK, "Ok");
		}
	}

}

