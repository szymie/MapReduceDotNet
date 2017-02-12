using System;
using System.Data;
using ServiceStack.CacheAccess;
using ServiceStack.Common;
using ServiceStack.FluentValidation;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints.Support;

namespace EntryPoint
{
	public abstract class BaseValidator<T> : AbstractValidator<T>, IDisposable
	{
		private IDbConnection db;

		public virtual IDbConnection Db
		{
			get
			{
				//AppHost
				return db ?? (db = HttpListenerBase.Instance.Container.TryResolve<IDbConnectionFactory>().Open());
			}
		}
			
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

		protected int getCurrentUserId()
		{
			var s = SessionFeature.GetOrCreateSession<AuthUserSession>(CacheClinet);

			/*var sessionKey = SessionFeature.GetSessionKey();

			Console.WriteLine("2");

			var user = CacheClinet.Get<AuthUserSession>(sessionKey);

			int uid;
			if (!int.TryParse(user.UserAuthId, out uid))
				throw new Exception("Error");*/

			return 0;
		}

		public virtual void Dispose()
		{
			if (db != null)
			{
				db.Dispose();
			}
		}
	}
}

