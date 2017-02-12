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
	public class StatusService : BaseService
	{
		public StatusService()
		{
			Db.CreateTableIfNotExists<Task>();
		}

		public object Get(StatusDto request)
		{
			var task = Db.Select<Task>(entity => entity.OwnerId == GetCurrentAuthUserId() && entity.Id == request.TaskId)
						  .FirstOrDefault();

			if (task != null)
			{
				var response = new StatusDtoResponse()
				{
					Status = task.Status
				};

				if (response.Status != "failed")
				{
					return response;
				}
				else
				{
					response.FailureMessage = Db.Select<Failure>(e => e.TaskId == task.Id).First().Message;						
					return response;
				}
			}
			else
			{
				return new HttpResult(HttpStatusCode.NotFound, "Not found");
			}
		}
	}
}
