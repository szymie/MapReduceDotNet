using System;
using System.IO;

namespace Worker
{
	public class LocalFilesDirectory
	{
		private string rootFolder = "/tmp/";
		
		public string DirectoryPath{ get; set;}

		public LocalFilesDirectory(WorkerConfig workerConfig){
			DirectoryPath += String.Format(rootFolder + "{0}-{1}-{2}/", workerConfig.WorkConfig.TaskId, workerConfig.CoordinatorId, workerConfig.WorkerId);
		}

		public void createDirectory(){
			try{
				Directory.CreateDirectory (DirectoryPath);
			}catch(Exception e){}
		}

		public void removeDirectory(){
			try{
				Directory.Delete(DirectoryPath, true);
			}catch(Exception e){}
		}
	}
}

