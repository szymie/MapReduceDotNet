using System;
using MapReduceDotNetLib;
using System.Threading;

namespace ClientLib
{
	public class MyReduce : Reduce
	{
		public int Counter{ get; set; } = 0;

		public override void reduce (string key, LineReader lineReader)
		{
			String line;
			while((line = lineReader.readLine()) != null){
				emit (Counter.ToString() + "dsdfsdf");
				Counter += 1;
			}
		}
	}
}

