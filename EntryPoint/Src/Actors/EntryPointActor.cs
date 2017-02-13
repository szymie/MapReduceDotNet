﻿using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.Data;
using ServiceStack.OrmLite;
using ServiceStack.WebHost.Endpoints.Support;
using System.Linq;

namespace EntryPoint
{
	public class EntryPointActor : TypedActor,IHandle<NewTaskRequestMessage>, IHandle<TaskFinishedMessage>, IHandle<TaskFailureMessage>, IDisposable
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

		public EntryPointActor()
		{
			Db.CreateTableIfNotExists<Failure>();
			MasterActor = getMasterActorRef();
		}

		public void Handle(NewTaskRequestMessage message)
		{
			initNewTask(message);
			fillAssembly(message);
			fillInputFiles(message);

			MasterActor.Tell(NewTask);
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
				InputFiles = new List<S3ObjectMetadata>()
			};
		}

		private void fillAssembly(NewTaskRequestMessage message)
		{
			NewTask.Assembly = new MapReduceDotNetLib.AssemblyMetadata()
			{
				Namespace = message.Assembly.Namespace,
				MapClassName = message.Assembly.MapClassName,
				ReduceClassName = message.Assembly.ReduceClassName,
				File = new S3ObjectMetadata("map-reduce-dot-net", $"{message.UserId}-assembly-{message.Assembly.Id}")
			};
		}

		private void fillInputFiles(NewTaskRequestMessage message)
		{
			foreach(int inputFileId in message.InputFileIds)
			{
				NewTask.InputFiles.Add(new S3ObjectMetadata("map-reduce-dot-net", $"{message.UserId}-input-{inputFileId}"));
			}
		}

		public void Handle(TaskFinishedMessage message)
		{
			changeTaskStatus(message.TaskId, "finished");

			foreach(KeyValuePair<string, S3ObjectMetadata> result in message.reduceResult)
			{
				var resultMetadata = new ResultMetadata()
				{
					Key = result.Key,
					Name = result.Value.Filename,
					TaskId = message.TaskId
				};

				Db.Save(resultMetadata);
			}
		}

		private void changeTaskStatus(int id, string status)
		{
			var task = Db.Select<Task>(e => e.Id == id).First();
			task.Status = status;
			Db.Update(task);

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
		}

		public virtual void Dispose()
		{
			if(db != null)
			{
				db.Dispose();
			}
		}
	}
}