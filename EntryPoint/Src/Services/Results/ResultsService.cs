using System;
using System.IO;
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
	public class ResultsService : BaseService
	{
		public ResultsService()
		{
			Db.CreateTableIfNotExists<Task>();
			Db.CreateTableIfNotExists<InputFileMetadata>();
			Db.CreateTableIfNotExists<AssemblyMetadata>();
			Db.CreateTableIfNotExists<ResultMetadata>();
		}

		public object Get(ResultsDto request)
		{
			var found = Db.Select<Task>(entity => entity.OwnerId == GetCurrentAuthUserId() && entity.Id == request.TaskId);

			if (found.Count != 0)
			{
				var entities = Db.Select<ResultMetadata>(entity => entity.TaskId == request.TaskId);
				return new ResultsDtoResponse(entities);
			}
			else
			{
				return new HttpResult(HttpStatusCode.NotFound, "Not found");
			}
		}

		public object Get(ResultDto request)
		{
			var found = Db.Select<Task>(e => e.OwnerId == GetCurrentAuthUserId() && e.Id == request.TaskId);

			if(found.Count != 0)
			{
				var entity = Db.Select<ResultMetadata>(e => e.TaskId == request.TaskId && e.Id == request.Id).FirstOrDefault();

				if(entity != null)
				{
					var S3Object = new S3ObjectMetadata(Environment.GetEnvironmentVariable("S3_BUCKET_NAME"), entity.Name);

					//var tmpFileName = $"/tmp/{S3Object.Filename}";

					S3Object.downStream().CopyTo(Response.OutputStream);

					//using (var outputFileStream = File.Create(tmpFileName))
					//{
					//	S3Object.downStream().CopyTo(outputFileStream);
					//}

					//var result = new HttpResult(new FileInfo(tmpFileName), true);

					//File.Delete(tmpFileName);

					return Response;
				}
				else
				{
					return new HttpResult(HttpStatusCode.NotFound, "Not found");
				}
			}
			else
			{
				return new HttpResult(HttpStatusCode.NotFound, "Not found");
			}
		}
	}
}
