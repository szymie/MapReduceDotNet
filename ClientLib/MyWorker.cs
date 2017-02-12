﻿using System;
using MapReduceDotNetLib;
using System.Threading;

namespace ClientLib
{
	public class MyWorker : Map
	{					
		public int Counter{ get; set; } = 0;

		public override void map (string key, LineReader lineReader)
		{
			String line;
			while((line = lineReader.readLine()) != null){
				emit ("key:"+Counter%2, line + "; value:" + Counter);
				Counter += 1;
			}
		}
	}
}

