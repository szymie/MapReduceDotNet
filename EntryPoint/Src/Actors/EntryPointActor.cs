using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.Data;
using ServiceStack.OrmLite;
using ServiceStack.WebHost.Endpoints.Support;
using System.Linq;
using System.Text.RegularExpressions;


namespace EntryPoint
{
	public class EntryPointActor : TypedActor,IHandle<NewTaskRequestMessage>, IHandle<TaskFinishedMessage>, IHandle<TaskFailureMessage>, IHandle<TaskAbortRequestMessage>, IDisposable
	{
		private IDbConnection db;
		public virtual IDbConnection Db
		{
			get
			{
				return db ?? (db = HttpListenerBase.Instance.Container.TryResolve<IDbConnectionFactory>().Open());
			}
		}

		private NewTaskMessage NewTask { get; set; }
		private ActorSelection MasterActor { get; set; }

		private Dictionary<int, string> usernameOfTask;

		public EntryPointActor()
		{
			Db.CreateTableIfNotExists<ResultMetadata>();
			Db.CreateTableIfNotExists<Failure>();
			MasterActor = getMasterActorRef();
			MasterActor.Tell(new RegisterEntryPointMessage());
		}

		public void Handle(NewTaskRequestMessage message)
		{
			initNewTask(message);
			fillAssembly(message);
			fillInputFiles(message);

			MasterActor.Tell(NewTask);

			usernameOfTask.Add(message.TaskId, message.Username);
		}

		private ActorSelection getMasterActorRef()
		{
			string masterAddress = Environment.GetEnvironmentVariable("MASTER_ADDRESS");

			if (masterAddress == null)
			{
				masterAddress = "localhost:8081";
				Console.WriteLine("No MASTER_ADDRESS found.");
			}

			Console.WriteLine("MASTER_ADDRESS {0}", masterAddress);

			return Context.ActorSelection("akka.tcp://MasterSystem@" + masterAddress + "/user/MasterActor");
		}

		private void initNewTask(NewTaskRequestMessage message)
		{
			NewTask = new NewTaskMessage()
			{
				M = message.M,
				R = message.R,
				TaskId = message.TaskId,
				Username = message.Username,
				InputFiles = new Dictionary<string, S3ObjectMetadata>()
			};
		}

		private void fillAssembly(NewTaskRequestMessage message)
		{
			NewTask.Assembly = new MapReduceDotNetLib.AssemblyMetadata()
			{
				Namespace = message.Assembly.Namespace,
				MapClassName = message.Assembly.MapClassName,
				ReduceClassName = message.Assembly.ReduceClassName,
				File = new S3ObjectMetadata(Environment.GetEnvironmentVariable("S3_BUCKET_NAME"), $"{message.UserId}-assembly-{message.Assembly.Id}")
			};
		}

		private void fillInputFiles(NewTaskRequestMessage message)
		{
			var inputFiles = new List<InputFileMetadata>();

			foreach (var fileId in message.InputFileIds)
			{
				var inputFile = Db.Select<InputFileMetadata>(entity => entity.Id == fileId).First();
				inputFiles.Add(inputFile);
			}

			foreach(var inputFile in inputFiles)
			{
				NewTask.InputFiles.Add(inputFile.Name,
					new S3ObjectMetadata(Environment.GetEnvironmentVariable("S3_BUCKET_NAME"), $"{message.UserId}-input-{inputFile.Id}"));
			}
		}

		public void Handle(TaskFinishedMessage message)
		{
			changeTaskStatus(message.TaskId, "finished");

			foreach(Tuple<S3ObjectMetadata, List<string>> result in message.reduceResult)
			{
				var resultMetadata = new ResultMetadata()
				{
					Keys = result.Item2,
					Name = result.Item1.Filename,
					TaskId = message.TaskId
				};

				Db.Save(resultMetadata);
			}

			usernameOfTask.Remove(message.TaskId);

			replyWithAck(message.TaskId);
		}

		private void changeTaskStatus(int id, string status)
		{
			var task = Db.Select<Task>(e => e.Id == id).First();
			task.Status = status;
			Db.Update(task);
		}

		private void deleteUnusedFilesAfterFinished(int taskId)
		{
			var bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
			var S3Bucket = new S3Bucket(bucketName);

			S3Bucket.fetchKeys();

			var filePattern = $"{usernameOfTask[taskId]}-{taskId}-(\\d+)-(\\d+)-(\\d+)-(\\d+)";

			Regex regex = new Regex(filePattern);

			while (S3Bucket.moveNext())
			{
				var currentKey = S3Bucket.getCurrentKey();

				if (regex.IsMatch(currentKey))
				{
					var S3Object = new S3ObjectMetadata(bucketName, currentKey);
					S3Object.remove();
				}
			}
		}


		private void replyWithAck(int taskId)
		{
			var ack = new TaskReceivedAckMessage() { TaskId = taskId };
			MasterActor.Tell(ack);
		}



		public void Handle(TaskFailureMessage message)
		{
			changeTaskStatus(message.TaskId, "failed");

			var failure = new Failure()
			{
				TaskId = message.TaskId,
				Message = message.Message
			};

			Db.Save(failure);

			deleteUnusedFilesAfterFinished(message.TaskId);

			usernameOfTask.Remove(message.TaskId);

			replyWithAck(message.TaskId);
		}

		private void deleteUnusedFilesAfterFailure(int taskId)
		{
			var bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
			var S3Bucket = new S3Bucket(bucketName);

			S3Bucket.fetchKeys();

			var filePattern = $"{usernameOfTask[taskId]}-{taskId}";

			while (S3Bucket.moveNext())
			{
				var currentKey = S3Bucket.getCurrentKey();

				if (currentKey.StartsWith(filePattern, StringComparison.CurrentCulture))
				{
					var S3Object = new S3ObjectMetadata(bucketName, currentKey);
					S3Object.remove();
				}
			}
		}

		public virtual void Dispose()
		{
			if(db != null)
			{
				db.Dispose();
			}
		}

		public void Handle(TaskAbortRequestMessage message)
		{
			MasterActor.Tell(new TaskAbortMessage() { TaskId = message.TaskId });
		}
	}
}
