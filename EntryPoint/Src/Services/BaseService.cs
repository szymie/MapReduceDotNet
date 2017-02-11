using System;
using System.Net;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;


namespace EntryPoint
{
	public class BaseService : Service
	{
		protected int GetCurrentAuthUserId()
		{
			var session = this.GetSession();

			if (!session.IsAuthenticated)
			{
				throw new HttpError(HttpStatusCode.Unauthorized, "Not authorized");
			}

			int id;

			if (!int.TryParse(session.UserAuthId, out id))
			{
				throw new Exception("Error");
			}

			return id;
		}
	}
}

