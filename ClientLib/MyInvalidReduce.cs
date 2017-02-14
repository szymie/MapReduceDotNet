using System;
using MapReduceDotNetLib;
using System.Threading;

namespace ClientLib
{
	public class MyInvalidReduce : Reduce
	{
		public override void reduce (string key, LineReader lineReader)
		{
			if (key.Equals ("asdf")) {
				throw new NotImplementedException ("dfadfasdfasfd");
			} else {
				Thread.Sleep (10000);
			}				
		}
	}
}

