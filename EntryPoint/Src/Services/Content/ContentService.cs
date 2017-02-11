using System;
using System.IO;
using System.Net;
using ServiceStack.Common.Web;
using System.Linq;
using ServiceStack.Common;
using ServiceStack.OrmLite;
using MapReduceDotNetLib;

namespace EntryPoint
{
	public class ContentService : BaseService
	{
		private const string all = "0";

		public ContentService()
		{
			Db.CreateTableIfNotExists<InputFileMetadata>();
			Db.CreateTableIfNotExists<AssemblyMetadata>();
		}

		public object Put(InputFileContentDto request)
		{
			if(request.Id == null)
			{
				var segments = base.Request.PathInfo.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				request.Id = int.Parse(segments[3]);
			}

			var entity = Db.Select<InputFileMetadata>(e => e.OwnerId == GetCurrentAuthUserId() && e.Id == request.Id)					
						   .FirstOrDefault();

			if(entity != null)
			{
				var objectKey = $"{GetCurrentAuthUserId()}-input-{entity.Id}";
				var S3Object = new S3ObjectMetadata("map-reduce-dot-net", objectKey);

				var memoryStream = new MemoryStream();
				request.RequestStream.CopyTo(memoryStream);

				S3Object.upStream(memoryStream);

				entity.IsUploaded = true;
				Db.Save(entity);

				return new HttpResult(HttpStatusCode.OK, "Ok");
			}
			else
			{
				return new HttpResult(HttpStatusCode.BadRequest, "Resource does not exist or you are not its owner");
			}
		}
	}
}
