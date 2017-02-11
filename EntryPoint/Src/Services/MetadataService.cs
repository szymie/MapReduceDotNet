using System;
using System.Net;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

using ServiceStack.Common;
using ServiceStack.OrmLite;
using ServiceStack.ServiceHost;

namespace EntryPoint
{
	public class MetadataService : BaseService
	{
		public MetadataService()
		{
			Db.CreateTableIfNotExists<InputFileMetadata>();
			Db.CreateTableIfNotExists<AssemblyMetadata>();
		}

		public object Post(InputFileMetadataDto request)
		{
			var newInputFileMetadata = new InputFileMetadata().PopulateWith(request);

			newInputFileMetadata.CreatedAt = DateTime.Now;
			//newInputFileMetadata.OwnerId = GetCurrentAuthUserId();

			Db.Insert(newInputFileMetadata);
			var id = (int) Db.GetLastInsertId();

			request.PopulateWith(newInputFileMetadata);
			request.Id = id;

			return new HttpResult(request)
			{
				StatusCode = HttpStatusCode.Created,
				Headers =
				{
					{ HttpHeaders.Location, base.Request.AbsoluteUri.CombineWith(id) }
				}
			};
		}

		public object Post(AssemblyMetadataDto request)
		{
			var newMetadata = new AssemblyMetadata().PopulateWith(request);

			newMetadata.CreatedAt = DateTime.Now;
			//newInputFileMetadata.OwnerId = GetCurrentAuthUserId();

			Db.Insert(newMetadata);
			var id = (int)Db.GetLastInsertId();

			request.PopulateWith(newMetadata);
			request.Id = id;

			return new HttpResult(request)
			{
				StatusCode = HttpStatusCode.Created,
				Headers =
				{
					{ HttpHeaders.Location, base.Request.AbsoluteUri.CombineWith(id) }
				}
			};
		}
	}
}
