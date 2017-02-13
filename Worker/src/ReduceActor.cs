﻿using System;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.IO;

namespace Worker
{
	public class ReduceActor : WorkerActor
	{
		private Reduce Reduce{ get; set; }

		protected override void workProcessing (){
			Reduce = (Reduce) loadClientAssembly (AssemblyMetaData.ReduceClassName);

			var filesToProcess = WorkerConfig.WorkConfig.FilesToProcess;
			foreach (KeyValuePair<string, List<S3ObjectMetadata>> entry in filesToProcess) {
				foreach(S3ObjectMetadata file in entry.Value){
					using (Stream fileStream = file.downStream ()) {
						LineReader lineReader = new LineReader (fileStream);

						Reduce.reduce (entry.Key, lineReader);
					}
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

			Coordinator.Tell(new ReduceWorkFinishedMessage (WorkerId, TaskId, Reduce.EmittedKeys, resultS3Object), self);
		}
	}
}

