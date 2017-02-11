using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.IO;
using System;
using System.Web;
using ServiceStack.Common.Web;
using EntryPoint;

namespace Server.Services
{
	[Route ("/api/file/{Id}", "Post")]
	public class FileUpload : IRequiresRequestStream
	{
		public string Id { get; set; }
		public Stream RequestStream { get; set; }
	}

	[Route ("/api/file", "POST")]
	public class FileMetadata : IReturn<FileMetadataResponse>{
		public string Name {get; set;}
	}

	public class FileMetadataResponse{
		public string Url { get; set; }
		public FileMetadataResponse(string url){
			Url = url;
		}
	}

	public class TestFileUploadService : BaseService
	{
		public object Post (FileUpload request)
		{
			try{
				string parentPath = base.Request.GetParentPathUrl();
				string path = base.Request.GetPathUrl();

				string id = path.Substring(parentPath.Length + 1, path.Length - parentPath.Length - 1);

				var outputStream = File.Open("/tmp/" + id, FileMode.Create);

				request.RequestStream.CopyTo(outputStream);
				outputStream.Close();
				return new HttpResult ("Uploaded", System.Net.HttpStatusCode.Created);
			}
			catch(Exception e){
				return new HttpResult ("File not found", System.Net.HttpStatusCode.NotFound);
			}

		}
	}

	public class FileMetadataService : BaseService{
		public object Post(FileMetadata request){
			string url = "/tmp/" + request.Name;

			var outputStream = File.Create(url);

			outputStream.Close();

			return new FileMetadataResponse ("localhost:8888/api/files/" + request.Name);
		}
	}

}