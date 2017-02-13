using System;
using System.IO;
using System.Collections.Generic;

namespace MapReduceDotNetLib
{
	public abstract class Reduce : Work
	{
		public string UserCreatedFile{ get; set;}

		public List<string> EmittedKeys{ get; set; } = new List<string>();

		private string currentKey;

		public abstract void reduce (string key, LineReader lineReader);

		protected void emit (string value){
			EmittedKeys.Add (currentKey);
			File.AppendAllText (UserCreatedFile, value + "\n");
		}

		public override void setEmitParams (string localFolder)
		{
			LocalFolder = localFolder;
			UserCreatedFile = LocalFolder + "result";
		}

		public void setKey(string key){
			this.currentKey = key;
		}
	}
}

