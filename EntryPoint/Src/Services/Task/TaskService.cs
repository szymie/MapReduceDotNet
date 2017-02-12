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
	public class TaskService : BaseService
	{
		public TaskService()
		{
			Db.CreateTableIfNotExists<Task>();
		}

		public object Post(TaskDto request)
		{

			var entity = new Task().PopulateWith(request);

			entity.CreatedAt = DateTime.Now;
			entity.OwnerId = GetCurrentAuthUserId();

			Db.Insert(entity);
			var id = (int) Db.GetLastInsertId();

			request.PopulateWith(entity);
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
