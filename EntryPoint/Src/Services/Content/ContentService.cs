using System;
using System.IO;
using System.Net;
using ServiceStack.Common.Web;
using System.Linq;
using ServiceStack.Common;
using ServiceStack.OrmLite;
using MapReduceDotNetLib;
using ServiceStack.ServiceInterface;

namespace EntryPoint
{
	[Authenticate]
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
				populateWithId(request);
			}

			return put<InputFileMetadata>(request, "input");
		}

		public object Put(AssemblyContentDto request)
		{
			if (request.Id == null)
			{
				populateWithId(request);
			}

			return put<AssemblyMetadata>(request, "assembly");
		}

		private void populateWithId(ContentDto dto)
		{
			var segments = base.Request.PathInfo.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			dto.Id = int.Parse(segments[3]);
		}

		private HttpResult put<Entity>(ContentDto request, string type) where Entity : Metadata, new()
		{
			var entity = Db.Select<Entity>(e => e.OwnerId == GetCurrentAuthUserId() && e.Id == request.Id)
							   .FirstOrDefault();

			if (entity != null)
			{
				var objectKey = $"{GetCurrentAuthUserId()}-{type}-{entity.Id}";
				var S3Object = new S3ObjectMetadata("map-reduce-dot-net", objectKey);

				S3Object.upStream(request.RequestStream);

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
