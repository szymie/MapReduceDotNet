using System;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Worker
{
	public class ReduceActor : WorkerActor
	{
		private Reduce Reduce{ get; set; }
		private string UserKey{ get; set; }

		protected override void workProcessing ()
		{
			Reduce = (Reduce) loadClientAssembly (AssemblyMetaData.ReduceClassName);

			var filesToProcess = WorkerConfig.WorkConfig.FilesToProcess;
			foreach (KeyValuePair<string, S3ObjectMetadata> entry in filesToProcess) {
				using (Stream fileStream = entry.Value.downStream ()) {
					LineReader lineReader = new LineReader (fileStream);

					Reduce.reduce (UserKey, lineReader);
				}
			}
		}

		protected override void uploadResult ()
		{
			string username = WorkerConfig.WorkConfig.Username;
			string bucketName = WorkerConfig.WorkConfig.AssemblyMetaData.File.BucketName;
			S3ObjectMetadata resultS3Object = new S3ObjectMetadata (
				bucketName,
				String.Format ("{0}-{1}-{2}-{3}", username, TaskId, CoordinatorId, WorkerId)
			);

			resultS3Object.upStream (File.Open(Reduce.UserCreatedFile, FileMode.Open));

			new ReduceWorkFinishedMessage (WorkerId, TaskId, UserKey, resultS3Object);

			Coordinator.Tell(new ReduceWorkFinishedMessage(WorkerId, TaskId, ReduceKey, resultS3Object), self);
		}
	}
}

