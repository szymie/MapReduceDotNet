using System;
using System.Net;
using ServiceStack.Common.Web;
using System.Linq;
using ServiceStack.Common;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using MapReduceDotNetLib;

namespace EntryPoint
{
	[Authenticate]
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
			return saveNewMetadata(request, new InputFileMetadata().PopulateWith(request));
		}

		public object Post(AssemblyMetadataDto request)
		{
			return saveNewMetadata(request, new AssemblyMetadata().PopulateWith(request));
		}

		private HttpResult saveNewMetadata(MetadataDto metadataDto, Metadata metadata)
		{
			metadata.IsUploaded = false;
			metadata.CreatedAt = DateTime.Now;
			metadata.OwnerId = GetCurrentAuthUserId();

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
				return getById<InputFileMetadataDto, InputFileMetadata>(request.Id.Value);
			}
			else
			{
				return getAll<InputFileMetadataDto, InputFileMetadata>(request);
			}
		}

		public object Get(AssemblyMetadataDto request)
		{
			if(request.Id != null)
			{
				return getById<AssemblyMetadataDto, AssemblyMetadata>(request.Id.Value);
			}
			else
			{
				return getAll<AssemblyMetadataDto, AssemblyMetadata>(request);
			}
		}

		private MetadataDtoResponse<Dto> getAll<Dto, Entity>(Dto dto) where Dto : MetadataDto, new() where Entity : Metadata
		{
			var entities = Db.Select<Entity>(e => e.OwnerId == GetCurrentAuthUserId());
			             
			var dtos = entities.Select(e => new Dto().PopulateWith(e)).ToList();

			return new MetadataDtoResponse<Dto>(dtos);
		}

		private HttpResult getById<Dto, Entity>(int id) where Dto : MetadataDto, new() where Entity : Metadata
		{
			var dto = Db.Select<Entity>(e => e.OwnerId == GetCurrentAuthUserId() && e.Id == id)
						   .Select(e => new Dto().PopulateWith(e))
			               .FirstOrDefault();

			if (dto != null)
			{
				return new HttpResult(dto, HttpStatusCode.OK);
			}
			else
			{
				return new HttpResult(HttpStatusCode.NotFound, "Not found");
			}
		}

		public object Delete(InputFileMetadataDto request)
		{
			return deleteMetadata<InputFileMetadata>(request, "input");
		}

		public object Delete(AssemblyMetadataDto request)
		{
			return deleteMetadata<AssemblyMetadata>(request, "assembly");
		}

		private HttpResult deleteMetadata<Entity>(MetadataDto dto, string type) where Entity : Metadata, new()
		{
			var found = Db.Select<Entity>(e => e.OwnerId == GetCurrentAuthUserId() && e.Id == dto.Id);

			//TODO - transaction?
			if (found.Count != 0)
			{
				var inProgressTasks = Db.Select<Task>(entity => entity.Status == "in progres");

				if (found[0].CanDelete(inProgressTasks))
				{
					Db.DeleteById<Entity>(dto.Id);

					var S3Object = new S3ObjectMetadata(Environment.GetEnvironmentVariable("S3_BUCKET_NAME"), $"{GetCurrentAuthUserId()}-{type}-{dto.Id}");
					S3Object.remove();
				}
			}

			return new HttpResult(HttpStatusCode.NoContent, "No content");
		}
	}
}
