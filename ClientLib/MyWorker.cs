using System;
using MapReduceDotNetLib;

namespace ClientLib
{
	public class MyWorker : Map
	{

		public MyWorker ()
		{

		}
			
		public override void map (string key, LineReader lineReader)
		{
			emit ("", "");
			throw new NotImplementedException ();
		}
	}
}

