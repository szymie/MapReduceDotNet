﻿using System;
using System.IO;
using System.Net;
using ServiceStack.Common.Web;
using System.Linq;
using ServiceStack.Common;
using ServiceStack.OrmLite;
using MapReduceDotNetLib;
using ServiceStack.ServiceInterface;
using Akka.Actor;

namespace EntryPoint
{
	[Authenticate]
	public class TaskService : BaseService
	{
		public IActorRef EntryPointActor { get; set; }

		public TaskService()
		{
			Db.CreateTableIfNotExists<Task>();
			Db.CreateTableIfNotExists<InputFileMetadata>();
			Db.CreateTableIfNotExists<AssemblyMetadata>();
		}

		public object Post(TaskDto request)
		{
			validateNewTaskRequest(request);

			var entity = new Task().PopulateWith(request);

			entity.CreatedAt = DateTime.Now;
			entity.OwnerId = GetCurrentAuthUserId();
			entity.Status = "in progress";

			Db.Insert(entity);
			var id = (int) Db.GetLastInsertId();

			request.PopulateWith(entity);
			request.Id = id;

			startTask(request);

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
			Console.WriteLine("AssemblyId= " + request.AssemblyId);

			if (!isUploadedAndOwnedByCurrentUser<AssemblyMetadata>(request.AssemblyId))
			{
				throw new HttpError(HttpStatusCode.BadRequest, "Assembly does not exist or is not uploaded or you are not its owner");
			}

			foreach (int inputFileId in request.InputFileIds)
			{
				if (!isUploadedAndOwnedByCurrentUser<InputFileMetadata>(inputFileId))
				{
					throw new HttpError(HttpStatusCode.BadRequest, "Input file does not exist or is not uploaded or you are not its owner");
				}
			}
		}

		private void startTask(TaskDto request)
		{
			var newTask = new NewTaskRequestMessage()
			{
				TaskId = request.Id.Value,
				M = request.M,
				R = request.R,
				InputFileIds = request.InputFileIds,
				Assembly = Db.Select<AssemblyMetadata>(e => e.Id == request.AssemblyId).FirstOrDefault(),
				Username = this.GetSession().UserName,
				UserId = GetCurrentAuthUserId()
			};

			EntryPointActor.Tell(newTask);
		}

		public object Get(TaskDto request)
		{
			if (request.Id != null)
			{
				return getById(request.Id.Value);
			}
			else
			{
				return getAll(request);
			}
		}

		private HttpResult getAll(TaskDto dto)
		{
			var entities = Db.Select<Task>(e => e.OwnerId == GetCurrentAuthUserId());

			var dtos = entities.Select(e => new TaskDto().PopulateWith(e)).ToList();

			return new HttpResult(dtos, HttpStatusCode.OK);
		}

		private HttpResult getById(int id)
		{
			var dto = Db.Select<Task>(e => e.OwnerId == GetCurrentAuthUserId() && e.Id == id)
			            .Select(e => new TaskDto().PopulateWith(e))
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

		public object Delete(TaskDto request)
		{
			var entity = Db.Select<Task>(e => e.OwnerId == GetCurrentAuthUserId() && e.Id == request.Id.Value && request.Status == "in progress")
						.FirstOrDefault();

			if (entity != null)
			{
				EntryPointActor.Tell(new TaskAbortRequestMessage() { TaskId = entity.Id });
				entity.Status = "aborted";
				Db.Save(entity);
				return new HttpResult(HttpStatusCode.OK, "Task aborted");
			}
			else
			{
				return new HttpResult(HttpStatusCode.BadRequest, "Task does not exist or in not in progress");
			}
		}
	}
}
