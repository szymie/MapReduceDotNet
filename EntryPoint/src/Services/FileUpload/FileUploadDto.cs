using System;
using System.IO;
using ServiceStack.ServiceHost;

namespace EntryPoint
{
	public abstract class FileDto : IRequiresRequestStream
	{
		public Stream RequestStream { get; set; }
	}
}

