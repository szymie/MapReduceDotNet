using System;
using ServiceStack.Common.Web;
using System.Net;
using MapReduceDotNetLib;
using ServiceStack.ServiceInterface;

namespace EntryPoint
{
	public class AssemblyFileService : Service
	{
		public object Put(DataFileDto request)
		{

			var file = new S3ObjectMetadata("map-reduce-dot-net", "file1");

			Console.WriteLine(request.RequestStream.Length);

			//file.upStream(request.RequestStream);

			return new HttpResult(HttpStatusCode.OK, "Ok");
		}
	}

}

