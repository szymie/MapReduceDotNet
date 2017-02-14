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

		protected override void workProcessing (){
			Reduce = (Reduce) loadClientAssembly (AssemblyMetaData.ReduceClassName);

			var filesToProcess = WorkerConfig.WorkConfig.FilesToProcess;
			foreach (KeyValuePair<string, List<S3ObjectMetadata>> entry in filesToProcess) {
				LineReader lineReader = new LineReader (entry.Value);
				Reduce.setKey (entry.Key);

				Reduce.reduce (entry.Key, lineReader);

				lineReader.dispose ();
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

			Thread.Sleep(WorkerId * 2000);
			Coordinator.Tell(new ReduceWorkFinishedMessage (WorkerId, TaskId, Reduce.EmittedKeys, resultS3Object), self);
			Console.WriteLine ("MapWorkFinished sent");
		}
	}
}

