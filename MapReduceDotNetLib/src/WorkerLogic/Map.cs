using System;
using System.Collections.Generic;
using Akka.Actor;
using UserGeneratedKey = System.String;
using FileName = System.String;
using System.IO;

namespace MapReduceDotNetLib
{
	public abstract class Map : Work
	{
		public Dictionary<UserGeneratedKey, FileName> createdFiles = new Dictionary<UserGeneratedKey, FileName>();

		public abstract void map (string key, LineReader lineReader);

		protected void emit (string key, string value){
			string filename;
			if (!createdFiles.TryGetValue (key, out filename)) {
				int artificialKey = KeyGenerator.generateKey ();
				filename = String.Format (LocalFileIO.localFilesLocation + "{0}-{1}-{2}-{3}", TaskId, CoordinatorId, WorkerId, artificialKey);

				createdFiles.Add (key, filename);
			}

			File.AppendAllText (filename, value);
		}
	}
}

