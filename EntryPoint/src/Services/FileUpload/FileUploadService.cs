using System;
using System.IO;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;

namespace Server
{
	public abstract class FileUploadService : BaseService
	{
		//TODO jakos ladnie podawac bazowa lokalizacje plikow
		protected string baseFileDirectory = "/tmp/";
		/*
		public object Get(FileUploadDto request){
			return new HttpResult ("Not implemented", System.Net.HttpStatusCode.NotFound);
		}*/

		public void X()
		{

		}

		public object _post (FileUploadDto request)
		{			
			string id = getId();
			string directory = null;
			try{
				directory = getFileDirectory();
			}
			catch(Exception e){
				return "Not authorized";
			}

			try{
				Directory.CreateDirectory(directory);
				var outputStream = File.Open(directory + id, FileMode.Create);

				request.RequestStream.CopyTo(outputStream);
				outputStream.Close();
				return new HttpResult ("Uploaded", System.Net.HttpStatusCode.Created);
			}
			catch(Exception e){
				return new HttpResult ("File not found", System.Net.HttpStatusCode.NotFound);
			}
		}


		//TODO sanitize user input
		private string getId(){
			try{
				string parentPath = base.Request.GetParentPathUrl();
				string path = base.Request.GetPathUrl();

				return path.Substring(parentPath.Length + 1, path.Length - parentPath.Length - 1);
			}
			catch(Exception e){}

			return "";
		}

		protected virtual string getFileDirectory ()
		{
			return baseFileDirectory + GetCurrentAuthUserId() + "/";
		}
	}
}

