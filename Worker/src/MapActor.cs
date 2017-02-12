using System;
using MapReduceDotNetLib;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using Akka.Actor;

namespace Worker
{
	public class MapActor : WorkerActor
	{
		private Map map{ get; set; }
			
		protected override void workProcessing(){	
			map = (Map) loadClientAssembly (AssemblyMetaData.MapClassName);

			var filesToProcess = WorkerConfig.WorkConfig.FilesToProcess;
			foreach (KeyValuePair<string, S3ObjectMetadata> entry in filesToProcess) {
				string filename = entry.Key;

				using (Stream fileStream = entry.Value.downStream ()) {
					LineReader lineReader = new LineReader (fileStream);

					map.map (filename, lineReader);
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

			Coordinator.Tell(new MapWorkFinishedMessage(WorkerId, TaskId, mapResult), self);
		}
	}
}

