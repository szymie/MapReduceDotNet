using System;
using MapReduceDotNetLib;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;

namespace Worker
{
	public class MapActor : WorkerActor
	{
		private WorkerConfig WorkerConfig{ get; set; }

		public MapActor ()
		{
		}
			
		public override void Handle (NewWorkerMessage message)
		{
			WorkerConfig = message.WorkerConfig;

			AssemblyMetadata assemblyMetaData = WorkerConfig.WorkConfig.AssemblyMetaData;
			Map map = loadClientMap (assemblyMetaData);

			Thread mapThread = new Thread (() => {
				var filesToProcess = WorkerConfig.WorkConfig.FilesToProcess;
				foreach (KeyValuePair<string, S3ObjectMetadata> entry in filesToProcess) {
					string filename = entry.Key;
					LineReader lineReader = new LineReader (entry.Value.downStream ());

					map.map(filename, lineReader);
				}
			});

			mapThread.Start ();
		}

		private Map loadClientMap(AssemblyMetadata assemblyMetadata){
			try{
				Assembly assembly = Assembly.LoadFrom(assemblyMetadata.Filename);
				Type type = assembly.GetType(assemblyMetadata.Namespace + "." + assemblyMetadata.MapClassName);
				return (Map) Activator.CreateInstance(type);
			}
			catch(Exception e){
				throw new NotImplementedException ();
			}
		}
	}
}

