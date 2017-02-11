using System;

namespace MapReduceDotNetLib
{
	public class UniqueKeyGenerator
	{
		private int key = 0;
		public int generateKey(){
			key = (key+1)%Int32.MaxValue;
			return key;
		}
	}
}

