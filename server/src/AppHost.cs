using Funq;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.MiniProfiler;
using ServiceStack.MiniProfiler.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Admin;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.Redis;
using ServiceStack.Logging;
using ServiceStack.Logging.NLogger;
using System.Configuration;

namespace Server
{
    public class AppHost : AppHostHttpListenerBase
    {
        private readonly bool m_debugEnabled = true;
        
        public AppHost ()
            : base ("Server HttpListener", typeof (AppHost).Assembly)
        {
        }

        public override void Configure (Container container)
        {
			LogManager.LogFactory = new NLogFactory ();
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            
            Plugins.Add (new AuthFeature (() => new AuthUserSession (),
                                          new IAuthProvider[] {new CredentialsAuthProvider ()})
                );
            Plugins.Add (new RegistrationFeature ());

			Plugins.Add(new RequestLogsFeature() {
				EnableSessionTracking = false,
				RequiredRoles = null
			});

			container.Register<ICacheClient>(new MemoryCacheClient());



			container.Register<IDbConnectionFactory>(c => new OrmLiteConnectionFactory(
				ConfigurationManager.ConnectionStrings["postgres"].ConnectionString,
				PostgreSqlDialect.Provider));

            container.Register<IDbConnectionFactory> (

                new OrmLiteConnectionFactory (@"Data Source=db.sqlite;Version=3;",
                                              SqliteOrmLiteDialectProvider.Instance)
                    {
                        ConnectionFilter = x => new ProfiledDbConnection (x, Profiler.Current)
                    });

            //Use OrmLite DB Connection to persist the UserAuth and AuthProvider info
            container.Register<IUserAuthRepository> (c => new OrmLiteAuthRepository (c.Resolve<IDbConnectionFactory> ()));

            Plugins.Add (new ValidationFeature ());
            container.RegisterValidators (typeof (AppHost).Assembly);

            var config = new EndpointHostConfig ();

            if (m_debugEnabled)
            {
                config.DebugMode = true; //Show StackTraces in service responses during development
                config.WriteErrorsToResponse = true;
                config.ReturnsInnerException = true;
            }

            SetConfig (config);
            CreateMissingTables (container);
        }

        private void CreateMissingTables (Container container)
        {
            var authRepo = (OrmLiteAuthRepository) container.Resolve<IUserAuthRepository> ();
            authRepo.CreateMissingTables ();
        }
    }
}