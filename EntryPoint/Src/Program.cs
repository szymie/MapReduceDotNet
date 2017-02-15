using System;
using System.IO;
using System.Threading;
using MapReduceDotNetLib;

namespace EntryPoint
{
	public class Program
	{
		public static void Main(string[] args)
		{
			/*var bucket = new S3Bucket("map-reduce-dot-net");

			bucket.fetchKeys();

			while(bucket.moveNext())
			{
				Console.WriteLine(bucket.getCurrentKey());
			}

			deleteUnusedFiles(1);*/

			var listeningOn = startEntryPoint(args);
			Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);
			ManualResetEvent resetEvent = new ManualResetEvent(false);
			resetEvent.WaitOne();
		}

		private static void deleteUnusedFiles(int taskId)
		{
			var bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
			var S3Bucket = new S3Bucket(bucketName);

			S3Bucket.fetchKeys();

			var filePattern = "szymie";

			while (S3Bucket.moveNext())
			{
				var currentKey = S3Bucket.getCurrentKey();

				if (currentKey.StartsWith(filePattern, StringComparison.CurrentCulture))
				{
					var S3Object = new S3ObjectMetadata(bucketName, currentKey);
					S3Object.remove();
				}
			}
		}

		private static string startEntryPoint(string[] args)
		{
			ushort port = 8888;

			if (args.Length == 1 && !ushort.TryParse(args[0], out port))
			{
				Console.Error.WriteLine("Wrong port number");
				Environment.Exit(2);
			}

			var appHost = new AppHost();
			appHost.Init();

			var listeningOn = string.Format("http://*:{0}/", port);
			appHost.Start(listeningOn);

			return listeningOn;
		}
	}
}
