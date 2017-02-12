using System;
using MapReduceDotNetLib;
using System.IO;

namespace EntryPoint
{
	public class Program
	{
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
