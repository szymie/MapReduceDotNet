using System;
using System.Collections.Generic;
using Akka.Actor;

namespace MapReduceDotNetLib
{
	public abstract class Worker
	{
		public Worker ()
		{
			
		}

		public abstract void map (string key, string value);
	}
}

