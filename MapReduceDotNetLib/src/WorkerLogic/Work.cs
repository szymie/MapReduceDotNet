using System;

namespace MapReduceDotNetLib
{
	public abstract class Work
	{
		protected string LocalFolder{ get; set;}
		protected UniqueKeyGenerator KeyGenerator = new UniqueKeyGenerator();

		public void setEmitParams (string localFolder)
		{
			this.LocalFolder = localFolder;
		}
	}
}

