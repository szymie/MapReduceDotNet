using System;
using MapReduceDotNetLib;

namespace ClientLib
{
	public class MyWorker : Worker
	{

		public MyWorker ()
		{

		}

		public override void map (string key, string value)
		{
			Console.WriteLine (key + " " + value);
				
		}
	}
}

