using System;
using Funq;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.MiniProfiler;
using ServiceStack.MiniProfiler.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.PostgreSQL;
using ServiceStack.Redis;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.ServiceInterface.Admin;
using ServiceStack.Logging;
using System.Data;

namespace EntryPoint
{
	public class AppHost : AppHostHttpListenerBase
	{
		private const string ENV_PG_IP = "PG_IP";
		private const string ENV_PG_USER = "PG_USER";
		private const string ENV_PG_DB = "PG_DB";
		private const string ENV_PG_PASS = "PG_PASS";
		private const string ENV_PG_PORT = "PG_PORT";
		private const string ENV_REDIS_IP = "REDIS_IP";
		private const string ENV_REDIS_PORT = "REDIS_PORT";

		private readonly bool debugEnabled = true;
		private string pgIp;
		private string pgUser;
		private string pgDb;
		private string pgPass;
		private string pgPort;
		private string redisIp;
		private string redisPort;
		private string pgConnectionString;
		private string redisConnectionString;

		public AppHost() : base("Server HttpListener", typeof(AppHost).Assembly)
		{
		}

		void LoadConfiguration()
		{
			/*pgIp = Environment.GetEnvironmentVariable(ENV_PG_IP);
			pgUser = Environment.GetEnvironmentVariable(ENV_PG_USER);
			pgDb = Environment.GetEnvironmentVariable(ENV_PG_DB);
			pgPass = Environment.GetEnvironmentVariable(ENV_PG_PASS);
			pgPort = Environment.GetEnvironmentVariable(ENV_PG_PORT);
			redisIp = Environment.GetEnvironmentVariable(ENV_REDIS_IP);
			redisPort = Environment.GetEnvironmentVariable(ENV_REDIS_PORT);*/
			pgIp = "localhost";
			pgUser = "postgres";
			pgDb = "postgres";
			pgPass = "postgres";
			pgPort = "5432";
			redisIp = pgIp;
			redisPort = "6379";

			pgConnectionString = string.Format("User ID={0};Password={1};Host={2};Port={3};Database={4};SSL=True",
				pgUser, pgPass, pgIp, pgPort, pgDb);
			redisConnectionString = string.Format("{0}:{1}", redisIp, redisPort);
		}

		public override void Configure(Container container)
		{
			LoadConfiguration();

			this.RequestFilters.Add((req, resp, requestDto) =>
			{
				ILog log = LogManager.GetLogger(GetType());
				log.Info(string.Format("REQ {0}: {1} {2} {3} {4} {5}", DateTimeOffset.Now.Ticks, req.HttpMethod,
					req.OperationName, req.RemoteIp, req.RawUrl, req.UserAgent));
			});

			this.ResponseFilters.Add((req, resp, dto) =>
			{
				ILog log = LogManager.GetLogger(GetType());
				log.Info(string.Format("RES {0}: {1} {2}", DateTimeOffset.Now.Ticks, resp.StatusCode,
					resp.ContentType));
			});

			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			Plugins.Add(new AuthFeature(() => new AuthUserSession(), 
			                            new IAuthProvider[] { new CredentialsAuthProvider() }));
			
			Plugins.Add(new RegistrationFeature());
			Plugins.Add(new SessionFeature());
			Plugins.Add(new RequestLogsFeature());

			container.Register<ICacheClient>(new MemoryCacheClient());
			/*container.Register<IRedisClientsManager>(c =>
				new PooledRedisClientManager(redisConnectionString));
			container.Register<ICacheClient>(c =>
				(ICacheClient)c.Resolve<IRedisClientsManager>()
				.GetCacheClient())
				.ReusedWithin(Funq.ReuseScope.None);*/

			var pgConnectionFactory = new OrmLiteConnectionFactory(pgConnectionString, PostgreSQLDialectProvider.Instance)
			{
				ConnectionFilter = x => new ProfiledDbConnection(x, Profiler.Current)
			};

			container.Register<IDbConnectionFactory>(pgConnectionFactory);

			//Use OrmLite DB Connection to persist the UserAuth and AuthProvider info
			container.Register<IUserAuthRepository>(c => new OrmLiteAuthRepository(c.Resolve<IDbConnectionFactory>()));

			Plugins.Add(new ValidationFeature());
			container.RegisterValidators(typeof(InputFileMetadataDtoValidator).Assembly);
			container.RegisterValidators(typeof(AssemblyMetadataDtoValidator).Assembly);
			container.RegisterValidators(typeof(TaskDtoValidator).Assembly);
			

			var config = new EndpointHostConfig();

			if (debugEnabled)
			{
				config.DebugMode = true;
				config.WriteErrorsToResponse = true;
				config.ReturnsInnerException = true;
			}

			SetConfig(config);
			CreateMissingTables(container);
		}


		private void CreateMissingTables(Container container)
		{
			var authRepository = (OrmLiteAuthRepository) container.Resolve<IUserAuthRepository>();
			authRepository.CreateMissingTables();
		}
	}
}
