using System;
using System.Net;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.OrmLite;

namespace EntryPoint
{
	public class BaseService : Service
	{
		protected int GetCurrentAuthUserId()
		{
			var session = this.GetSession();

			if(!session.IsAuthenticated)
			{				
				throw new HttpError(HttpStatusCode.Unauthorized, "Not authorized");
			}

			int id;

			if(!int.TryParse(session.UserAuthId, out id))
			{
				throw new Exception("Error");
			}

			return id;
		}
	
		protected bool existsAndIsOwnedByCurrentUser<U>(int id) where U : Entity, IOwnable
		{
			Console.WriteLine(Db == null);

			return Db.Select<U>(entity => entity.Id == id && entity.OwnerId == GetCurrentAuthUserId()).Count > 0;
		}

		protected bool exists<U>(int id) where U : Entity
		{
			return Db.Select<U>(entity => entity.Id == id).Count > 0;
		}
	}
}

