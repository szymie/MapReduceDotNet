using System;
using System.IO;

namespace MapReduceDotNetLib
{
	public abstract class Reduce : Work
	{
		public string UserCreatedFile{ get; set;}

		public abstract void reduce (string key, LineReader lineReader);

		protected void emit (string value){
			File.AppendAllText (UserCreatedFile, value + "\n");
		}

		public override void setEmitParams (string localFolder)
		{
			LocalFolder = localFolder;
			UserCreatedFile = LocalFolder + "result";
		}
	}
}

