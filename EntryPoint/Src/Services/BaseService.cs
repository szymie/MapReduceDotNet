using System;
using System.Net;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.OrmLite;
using System.Linq;

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
	
		protected bool existsAndIsOwnedByCurrentUser<U>(int id) where U : Entity, Ownable
		{
			return Db.Select<U>(entity => entity.Id == id && entity.OwnerId == GetCurrentAuthUserId()).Count > 0;
		}

		protected bool isUploadedAndOwnedByCurrentUser<U>(int id) where U : Entity, Uploadable
		{
			Console.WriteLine("GetCurrentAuthUserId= " + GetCurrentAuthUserId());

			var e =  Db.Select<U>(entity =>

				entity.Id == id && entity.OwnerId == GetCurrentAuthUserId()

							   ).First();


			Console.WriteLine("e.is= " + e.IsUploaded);
			Console.WriteLine("E= " + e.Id);
			       
			return Db.Select<U>(entity =>

				entity.Id == id && entity.OwnerId == GetCurrentAuthUserId() && entity.IsUploaded

			).Count > 0;
		}

		protected bool exists<U>(int id) where U : Entity
		{
			return Db.Select<U>(entity => entity.Id == id).Count > 0;
		}
	}
}

