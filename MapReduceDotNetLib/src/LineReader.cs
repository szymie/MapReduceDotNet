using System;
using System.IO;

namespace MapReduceDotNetLib
{
	public class LineReader
	{
		private StreamReader StreamReader{ get; set;}

		public LineReader (Stream stream)
		{
			this.StreamReader = new StreamReader (stream);
		}
		

		public string readLine (){
			return StreamReader.ReadLine ();
		}
	}
}

