using System;
using MapReduceDotNetLib;
using System.IO;

namespace EntryPoint
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var file0 = new S3ObjectMetadata("map-reduce-dot-net", "file0");

			using(FileStream fileToUpload = new FileStream("/tmp/file0", FileMode.Open, FileAccess.Read))
			{
				file0.upStream(fileToUpload);
			}

			Console.WriteLine("Press...");
			Console.ReadLine();

			Stream stream = file0.downStream();

			using(var fileStream = File.Create("/tmp/file1"))
			{
				stream.CopyTo(fileStream);
			}

			Console.WriteLine("end");
		}
	}
}
