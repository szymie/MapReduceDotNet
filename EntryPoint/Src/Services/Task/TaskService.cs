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
	public class TaskService : BaseService
	{
		public TaskService()
		{
			Db.CreateTableIfNotExists<Task>();
		}

		public object Post(TaskDto request)
		{
			validateNewTaskRequest(request);

			var entity = new Task().PopulateWith(request);

			entity.CreatedAt = DateTime.Now;
			entity.OwnerId = GetCurrentAuthUserId();

			Db.Insert(entity);
			var id = (int) Db.GetLastInsertId();

			request.PopulateWith(entity);
			request.Id = id;
			request.status = "in progress";

			//start processing

			return new HttpResult(request)
			{
				StatusCode = HttpStatusCode.Created,
				Headers =
				{
					{ HttpHeaders.Location, base.Request.AbsoluteUri.CombineWith(id) }
				}
			};
		}

		private void validateNewTaskRequest(TaskDto request)
		{
			if (!existsAndIsOwnedByCurrentUser<AssemblyMetadata>(request.AssemblyId))
			{
				throw new HttpError(HttpStatusCode.BadRequest, "Assembly does not exist or you are not its owner");
			}

			foreach (int inputFileId in request.InputFileIds)
			{
				if (!existsAndIsOwnedByCurrentUser<InputFileMetadata>(inputFileId))
				{
					throw new HttpError(HttpStatusCode.BadRequest, "Input file does not exist or you are not its owner");
				}
			}
		}
	}
}
