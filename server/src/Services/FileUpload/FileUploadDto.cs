using System;
using System.IO;
using ServiceStack.ServiceHost;

namespace Server{
	public class FileUploadDto : IRequiresRequestStream
	{
		public string Id { get; set; }
		public Stream RequestStream { get; set; }
	}
}

