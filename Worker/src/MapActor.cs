using System;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Worker
{
	public class MapActor : WorkerActor
	{
		private Map map{ get; set; }
			
		protected override void workProcessing(){	
			map = (Map) loadClientAssembly (AssemblyMetaData.MapClassName);

			var filesToProcess = WorkerConfig.WorkConfig.FilesToProcess;
			foreach (KeyValuePair<string, List<S3ObjectMetadata>> entry in filesToProcess) {
				foreach(S3ObjectMetadata file in entry.Value){
					LineReader lineReader = new LineReader (file);

					map.map (entry.Key, lineReader);

					lineReader.dispose ();
				}
			}

		}

		protected override void uploadResult ()
		{
			Dictionary<string, string> createdFiles = map.createdFiles;

			Dictionary<string, S3ObjectMetadata> mapResult = new Dictionary<string, S3ObjectMetadata> ();

			UniqueKeyGenerator keyGenerator = new UniqueKeyGenerator ();

			foreach (KeyValuePair<string, string> pair in createdFiles) {
				string username = WorkerConfig.WorkConfig.Username;
				string bucketName = WorkerConfig.WorkConfig.AssemblyMetaData.File.BucketName;
				S3ObjectMetadata resultS3Object = new S3ObjectMetadata (
	            	bucketName,
					String.Format ("{0}-{1}-{2}-{3}-{4}", username, TaskId, CoordinatorId, WorkerId, keyGenerator.generateKey())
	            );

				resultS3Object.upStream (File.Open(pair.Value, FileMode.Open));

				mapResult.Add (pair.Key, resultS3Object);
			}

			Console.WriteLine ("MapWorkFinished sending");
			Thread.Sleep(WorkerId * 2000);
			Coordinator.Tell(new MapWorkFinishedMessage(WorkerId, TaskId, mapResult), self);
			Console.WriteLine ("MapWorkFinished sent");
		}
	}
}

