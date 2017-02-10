using System;
using MapReduceDotNetLib;
using System.IO;

namespace EntryPoint
{
	public class Program
	{
		/*public static void Main(string[] args)
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
		}*/


		public static void Main(string[] args)
		{
			ushort port = 8888;

			if (args.Length == 1 && !ushort.TryParse(args[0], out port))
			{
				Console.Error.WriteLine("Wrong port number");
				Environment.Exit(2);
			}

			var appHost = new AppHost();
			appHost.Init();
			string listeningOn = string.Format("http://*:{0}/", port);
			appHost.Start(listeningOn);

			Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);
			Console.WriteLine("Press <ENTER> key to exit...");
			Console.ReadLine();
		}
	}
}
