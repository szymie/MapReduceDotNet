using System;
using MapReduceDotNetLib;
using System.Threading;
using System.Collections.Generic;

namespace ClientLib
{
	public class MyReduce : Reduce
	{
		public override void reduce (string key, LineReader lineReader)
		{
			String line;
			int counter = 0;
			while((line = lineReader.readLine()) != null){
				counter++;
			}

			emit(key + " : " + counter);
		}
	}
}

