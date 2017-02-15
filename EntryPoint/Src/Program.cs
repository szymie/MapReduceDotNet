using System;
using System.IO;
using System.Threading;
using MapReduceDotNetLib;
using System.Text.RegularExpressions;

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

			var taskId = 1;
			var filePattern = $"szymie-{taskId}-(\\d+)-(\\d+)-(\\d+)-(\\d+)";
			Regex regex = new Regex(filePattern);

			deleteUnusedFiles(key => regex.IsMatch(key));*/

			var listeningOn = startEntryPoint(args);
			Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);
			ManualResetEvent resetEvent = new ManualResetEvent(false);
			resetEvent.WaitOne();
		}

		private static void deleteUnusedFiles(Func<string, bool> matches)
		{
			var bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
			var S3Bucket = new S3Bucket(bucketName);

			S3Bucket.fetchKeys();

			while (S3Bucket.moveNext())
			{
				var currentKey = S3Bucket.getCurrentKey();

				if (matches.Invoke(currentKey))
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
