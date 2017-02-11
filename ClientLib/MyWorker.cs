using System;
using MapReduceDotNetLib;

namespace ClientLib
{
	public class MyWorker : Map
	{					
		public int Counter{ get; set; } = 0;

		public override void map (string key, LineReader lineReader)
		{
			String line;
			while((line = lineReader.readLine()) != null){
				emit ("myKey", "value:" + Counter);
				Counter += 1;
			}
				
			//throw new NotImplementedException ();
		}
	}
}

