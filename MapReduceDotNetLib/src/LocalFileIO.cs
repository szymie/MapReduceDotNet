using System;
using System.IO;
using System.Reflection;

namespace MapReduceDotNetLib
{
	public class LocalFileIO
	{
		public static string localFilesLocation = "/tmp/";

		/*
		public static void writeStream(string filename, Stream inputStream){
			using (inputStream) {
				using(FileStream localFileStream = File.Create (localFilesLocation + filename)){
					inputStream.CopyTo (localFileStream);
				}
			}	
		}

		public static Assembly loadAssembly(string assemblyFileName){
			return Assembly.LoadFrom(localFilesLocation + assemblyFileName);
		}

		public static void appendAllText(string filename, string text){

		}*/
	}
}

