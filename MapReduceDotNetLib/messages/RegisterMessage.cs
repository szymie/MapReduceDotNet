using System;

namespace MapReduceDotNetLib
{
	public class RegisterMessage
	{
		public RegisterMessage (string data)
		{							
			Data = data;			
		}

		public string Data { get;private set; }
	}
}

