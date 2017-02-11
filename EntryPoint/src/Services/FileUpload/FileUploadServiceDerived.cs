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
			Console.WriteLine("1");

			var file = new S3ObjectMetadata("map-reduce-dot-net", "file1");

			var ms = new MemoryStream();
			request.RequestStream.CopyTo(ms);

			file.upStream2(ms);

			Console.WriteLine("2");

			return new HttpResult(HttpStatusCode.OK, "Ok");
		}
	}

}

