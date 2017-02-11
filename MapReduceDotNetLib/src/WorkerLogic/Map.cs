using System;
using System.Collections.Generic;
using Akka.Actor;
using UserGeneratedKey = System.String;
using FileName = System.String;
using System.IO;

namespace MapReduceDotNetLib
{
	public abstract class Map
	{
		private int TaskId{ get; set; }
		private int WorkerId{ get; set; }
		private UniqueKeyGenerator KeyGenerator = new UniqueKeyGenerator();

		public Dictionary<UserGeneratedKey, FileName> createdFiles = new Dictionary<UserGeneratedKey, FileName>();

		public void setEmitParams (int taskId, int workerId)
		{
			this.TaskId = taskId;
			this.WorkerId = workerId;
		}

		public abstract void map (string key, LineReader lineReader);

		protected void emit (string key, string value){
			string filename;
			if (!createdFiles.TryGetValue (key, out filename)) {
				int artificialKey = KeyGenerator.generateKey ();
				filename = TaskId + "-map-" + WorkerId + "-" + artificialKey;

				createdFiles.Add (key, filename);
			}

			using(StreamWriter streamWriter = File.AppendText (filename)){
				streamWriter.WriteLine (value);
			}
		}
	}
}

