using System;
using System.Net;
using ServiceStack.Common.Web;
using System.Linq;
using ServiceStack.Common;
using ServiceStack.OrmLite;

namespace EntryPoint
{
	public class MetadataService : BaseService
	{
		private const string all = "0";

		public MetadataService()
		{
			Db.CreateTableIfNotExists<InputFileMetadata>();
			Db.CreateTableIfNotExists<AssemblyMetadata>();
		}

		public object Post(InputFileMetadataDto request)
		{
			return saveMetadataDto(request, new InputFileMetadata().PopulateWith(request));
		}

		public object Post(AssemblyMetadataDto request)
		{
			return saveMetadataDto(request, new AssemblyMetadata().PopulateWith(request));
		}

		private HttpResult saveMetadataDto(MetadataDto metadataDto, Metadata metadata)
		{
			metadata.IsUploaded = false;
			metadata.CreatedAt = DateTime.Now;
			//newInputFileMetadata.OwnerId = GetCurrentAuthUserId();

			Db.Insert(metadata);
			var id = (int) Db.GetLastInsertId();

			metadataDto.PopulateWith(metadata);
			metadataDto.Id = id;

			return new HttpResult(metadataDto)
			{
				StatusCode = HttpStatusCode.Created,
				Headers =
				{
					{ HttpHeaders.Location, base.Request.AbsoluteUri.CombineWith(id) }
				}
			};
		}

		public object Get(InputFileMetadataDto request)
		{
			if(request.Id != null)
			{
				return getMetadataById<InputFileMetadataDto, InputFileMetadata>(request.Id.Value);
			}
			else
			{
				return getMetadata<InputFileMetadataDto, InputFileMetadata>(request);
			}
		}

		public object Get(AssemblyMetadataDto request)
		{
			if(request.Id != null)
			{
				return getMetadataById<AssemblyMetadataDto, AssemblyMetadata>(request.Id.Value);
			}
			else
			{
				return getMetadata<AssemblyMetadataDto, AssemblyMetadata>(request);
			}
		}

		private MetadataDtoResponse<Dto> getMetadata<Dto, Entity>(Dto dto) where Dto : MetadataDto, new() where Entity : Metadata
		{
			var entities = Db.Select<Entity>(e => e.OwnerId == GetCurrentAuthUserId());
			             
			var dtos = entities.Select(e => new Dto().PopulateWith(e)).ToList();

			return new MetadataDtoResponse<Dto>(dtos);
		}

		private HttpResult getMetadataById<Dto, Entity>(int id) where Dto : MetadataDto, new() where Entity : Metadata
		{
			var entity = Db.Select<Entity>(e => e.OwnerId == GetCurrentAuthUserId() && e.Id == id)
						   .Select(e => new Dto().PopulateWith(e))
			               .FirstOrDefault();

			if (entity != null)
			{
				return new HttpResult(entity, HttpStatusCode.OK);
			}
			else
			{
				return new HttpResult(HttpStatusCode.NotFound); 
				//return HttpError.NotFound("Not found");
			}
		}

		public object Delete(InputFileMetadataDto request)
		{
			return deleteMetadata<InputFileMetadata>(request);
		}

		public object Delete(AssemblyMetadataDto request)
		{
			return deleteMetadata<AssemblyMetadata>(request);
		}

		private HttpResult deleteMetadata<Entity>(MetadataDto dto) where Entity : Metadata, new()
		{
			var found = Db.Select<Entity>(e => e.OwnerId == GetCurrentAuthUserId() && e.Id == dto.Id);

			if (found.Count != 0)
			{
				Db.DeleteById<Entity>(dto.Id); 
				//TODO - delete from S3
			}

			return new HttpResult { StatusCode = HttpStatusCode.NoContent };
		}
	}
}
