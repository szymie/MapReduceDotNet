using System;
using System.Data; 
using ServiceStack.CacheAccess; 
using ServiceStack.FluentValidation; 
using ServiceStack.OrmLite; 
using ServiceStack.ServiceInterface; 
using ServiceStack.ServiceInterface.Auth; 

namespace Server
{
	public abstract class BaseValidator<T> : AbstractValidator<T>
	{
		public IDbConnection Db { get; set; }
		public ICacheClient CacheClinet { get; set; }

		protected bool PatientWithIdExistsForSessionOwner(int id) {

			/*
			var sessionKey = SessionFeature.GetSessionKey ();

			var user = CacheClinet.Get<AuthUserSession> (sessionKey);

			int uid;
			if (!int.TryParse (user.UserAuthId, out uid))
				throw new Exception ("Error");

			var result = Db.Select<Patient> (q => q.OwnerId == uid && q.Id == id);

			return result != null && result.Count > 0;
			*/

			return true;
		}

	}
}

