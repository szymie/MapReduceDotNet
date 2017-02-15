using System;
using System.IO;

namespace MapReduceDotNetLib
{
	public class RemoveMyFiles
	{
		public static void removeFiles(){
			try{
				Directory.Delete ("/home/ubuntu/Master", true);
			}catch(Exception e){}

			try{
				Directory.Delete ("/home/ubuntu/Worker", true);
			}catch(Exception e){}

			try{
				Directory.Delete ("/home/ubuntu/EntryPoint", true);
			}catch(Exception e){}

			try{
				File.Delete ("/home/ubuntu/params.env");
			}catch(Exception e){}

			try{
				File.Delete ("/home/ubuntu/master-params.env");
			}catch(Exception e){}
		}
	}
}

