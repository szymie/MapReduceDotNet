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
		private WorkerConfig WorkerConfig{ get; set; }
		private int TaskId{ get; set; }
		private int CoordinatorId{ get; set; }
		private int WorkerId{ get; set; }

		public MapActor ()
		{
		}
			
		public override void Handle (NewWorkerMessage message)
		{
			WorkerConfig = message.WorkerConfig;
			TaskId = WorkerConfig.WorkConfig.TaskId;
			CoordinatorId = WorkerConfig.CoordinatorId;
			WorkerId = WorkerConfig.WorkerId;
			IActorRef sender = Sender;
			AssemblyMetadata assemblyMetaData = WorkerConfig.WorkConfig.AssemblyMetaData;

			Map map = loadClientMap (assemblyMetaData);

			Thread mapThread = new Thread (() => {
				var filesToProcess = WorkerConfig.WorkConfig.FilesToProcess;
				foreach (KeyValuePair<string, S3ObjectMetadata> entry in filesToProcess) {
					string filename = entry.Key;
					LineReader lineReader = new LineReader (entry.Value.downStream ());

					map.map(filename, lineReader);
				}

				Dictionary<string, S3ObjectMetadata> uploadedResult = uploadResult(map.createdFiles);
				sender.Tell(new MapWorkFinishedMessage(WorkerId, TaskId, uploadedResult));
			});

			mapThread.Start ();
		}

		private Dictionary<string, S3ObjectMetadata> uploadResult (Dictionary<string, string> createdFiles)
		{
			Dictionary<string, S3ObjectMetadata> mapResult = new Dictionary<string, S3ObjectMetadata> ();

			foreach (KeyValuePair<string, string> pair in createdFiles) {
				string username = WorkerConfig.WorkConfig.Username;
				string bucketName = WorkerConfig.WorkConfig.AssemblyMetaData.File.BucketName;
				S3ObjectMetadata resultFile = new S3ObjectMetadata (
	            	bucketName,
	            	String.Format ("{0}-{1}-{2}-{3}-{4}", username, TaskId, pair.Key, CoordinatorId, WorkerId)
	            );
				using (FileStream stream = File.Open(pair.Value, FileMode.Open)) {
					resultFile.upStream (stream);
				}
			}

			return mapResult;
		}

		private Map loadClientMap(AssemblyMetadata assemblyMetadata){
			string asseblyFileName = TaskId + "-map-" + WorkerId + "-assembly";

			try{
				using (Stream stream = assemblyMetadata.File.downStream ()) {
					FileStream fileStream = File.Create (asseblyFileName);
					stream.CopyTo (fileStream);
				}			

				Assembly assembly = Assembly.LoadFrom(asseblyFileName);
				Type type = assembly.GetType(assemblyMetadata.Namespace + "." + assemblyMetadata.MapClassName);
				Map map = (Map) Activator.CreateInstance(type);
				map.setEmitParams (TaskId, WorkerId);

				return map;
			}
			catch(Exception e){
				throw new NotImplementedException ();
			}
		}
	}
}

