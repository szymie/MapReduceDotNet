using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Master
{
	public class MasterActor : TypedActor, IHandle<RegisterMapCoordinatorMessage>, IHandle<NewWorkAckMessage>, IHandle<Terminated>, IHandle<MapWorkFinishedMessage>, IHandle<WorkerFailureMessage>,
	IHandle<RegisterReduceCoordinatorMessage>, IHandle<NewTaskMessage>, IHandle<DivideResponseMessage>
	{
		private List<Coordinator> validMapCoordinator = new List<Coordinator>();
		private List<Coordinator> validReduceCoordinator = new List<Coordinator>();

		private int mapCoordinatorsRoundRobinIndex = 0;
		private int reduceCoordinatorsRoundRobinIndex = 0;

		private Dictionary<int, Task> tasks = new Dictionary<int, Task> ();

		private UniqueKeyGenerator coordinatorKeyGenerator = new UniqueKeyGenerator();

		private IActorRef fileDivider = Context.System.ActorOf<DividerActor>("dividerActor");

		public void Handle (RegisterMapCoordinatorMessage message){
			int coordinatorId = coordinatorKeyGenerator.generateKey ();
			Coordinator coordinator = new Coordinator (coordinatorId, Sender, true);

			validMapCoordinator.Add (coordinator);
			Context.Watch (Sender);
			Sender.Tell (new RegisterCoordinatorAckMessage(coordinatorId));
		}

		public void Handle (RegisterReduceCoordinatorMessage message){
			int coordinatorId = coordinatorKeyGenerator.generateKey ();
			Coordinator coordinator = new Coordinator (coordinatorId, Sender, false);

			validReduceCoordinator.Add (coordinator);
			Context.Watch (Sender);
			Sender.Tell (new RegisterCoordinatorAckMessage(coordinatorId));
		}

		public void Handle (NewTaskMessage message){
			tasks.Add (message.TaskId, new Task(message, Sender));

			fileDivider.Tell(new DivideRequestMessage(message.M, message.InputFiles, message.TaskId, message.Username));
		}

		public void Handle(DivideResponseMessage message){
			Task task = tasks[message.TaskId];
			task.addDivideFiles(message.Files);

			foreach(Dictionary<string, S3ObjectMetadata> files in message.Files){
				Dictionary<string, List<S3ObjectMetadata>> workConfigFiles = new Dictionary<string, List<S3ObjectMetadata>> ();

				foreach(KeyValuePair<string, S3ObjectMetadata> pair in files){
					workConfigFiles.Add (pair.Key, new List<S3ObjectMetadata> (){pair.Value});
				}

				WorkConfig workConfig = new WorkConfig (
					task.Id,
					task.Username,
					workConfigFiles,
					task.AssemblyMetadata
				);

				startNewMapWork (task, workConfig);
			}
		}

		private void startNewMapWork(Task task, WorkConfig workConfig){
			Coordinator coordinator = getNextMapCoordinator ();
			Console.WriteLine ("started map work: " + task.Id + "-" + coordinator.Id);

			if(!task.MapCoordinators.ContainsKey(coordinator.CoordinatorActor)){
				task.MapCoordinators.Add(coordinator.CoordinatorActor, coordinator);
			}

			int orderedWorkConfigId = coordinator.storeOrderedWork (workConfig);
			coordinator.CoordinatorActor.Tell (new NewWorkMessage (orderedWorkConfigId, workConfig));
		}

		public void Handle (NewWorkAckMessage message){
			int orderedWorkId = message.OrderedWorkId;
			Task task;
			if (tasks.TryGetValue (message.TaskId, out task)) {
				Coordinator coordinator;
				bool isMapCoordinator;
				if (task.getCoordinatorByActorRef (Sender, out coordinator, out isMapCoordinator)) {
					WorkConfig orderedWorkConfig = coordinator.OrderedWorks [orderedWorkId];
					coordinator.OrderedWorks.Remove (orderedWorkId);

					Work work = new Work (task.Id, message.WorkerId, orderedWorkConfig);
					coordinator.Works.Add (message.WorkerId, work);

					if (isMapCoordinator) {
						Console.WriteLine ("map ack: " + task.Id + "-" + coordinator.Id + "-" + message.WorkerId);
					} else {
						Console.WriteLine ("reduce ack: " + task.Id + "-" + coordinator.Id + "-" + message.WorkerId);
					}
				}
				else {
					Console.WriteLine ("No coord found NewWorkAckMessage");
				}
			} 
		}

		public void Handle (MapWorkFinishedMessage message){
			Task task;
			Coordinator coordinator;


			if (tasks.TryGetValue (message.TaskId, out task)) {
				if (task.MapCoordinators.TryGetValue (Sender, out coordinator)) {
					Console.WriteLine ("MapWorkFinished: " + message.TaskId + "-" + coordinator.Id + "-" + message.WorkerId);

					task.addMapResult (message.MapResult);

					if (coordinator.Works.ContainsKey (message.WorkerId)) {
						coordinator.Works.Remove (message.WorkerId);
					}

					if (coordinator.countWorksForTask (task.Id) == 0) {
						task.MapCoordinators.Remove (Sender);
					}

					if (task.MapCoordinators.Count == 0) {
						startReduceProcessing (task);
					}
				} else {
					foreach (S3ObjectMetadata s3ObjectMetadata in message.MapResult.Values) {
						s3ObjectMetadata.remove ();
					}
					Console.WriteLine ("No coord found MapWorkFinishedMessage");
					
				}
			} else {
				foreach (S3ObjectMetadata s3ObjectMetadata in message.MapResult.Values) {
					s3ObjectMetadata.remove ();
				}
			}
		}

		private void startReduceProcessing (Task task){
			task.deleteDividerFiles ();

			List<WorkConfig> reduceWorkConfigs = createReduceConfigs (task);

			foreach (var workConfig in reduceWorkConfigs) {
				startNewReduceWork (task, workConfig);
			}
		}

		private void startNewReduceWork(Task task, WorkConfig workConfig){			
			Coordinator coordinator = getNextReduceCoordinator ();
			Console.WriteLine ("started reduce work: " + task.Id + "-" + coordinator.Id);

			int orderedWorkConfigId = coordinator.storeOrderedWork (workConfig);

			NewWorkMessage newWorkMessage = new NewWorkMessage (orderedWorkConfigId, workConfig);

			if(!task.ReduceCoordinators.ContainsKey(coordinator.CoordinatorActor)){
				task.ReduceCoordinators.Add(coordinator.CoordinatorActor, coordinator);
			}

			coordinator.CoordinatorActor.Tell (newWorkMessage);
		}

		private List<WorkConfig> createReduceConfigs(Task task){
			List<WorkConfig> reduceWorkConfigs = new List<WorkConfig> (task.R);
			for (int i = 0; i < task.R; i++) {
				reduceWorkConfigs.Add (new WorkConfig(task.Id, task.Username, new Dictionary<string, List<S3ObjectMetadata>>(), task.AssemblyMetadata));
			}

			var mapResultEnumerator = task.MapResult.GetEnumerator ();

			while (true) {
				foreach (WorkConfig workConfig in reduceWorkConfigs) {
					if (!mapResultEnumerator.MoveNext ()) {
						mapResultEnumerator.Dispose ();
						return reduceWorkConfigs;
					}

					KeyValuePair<string, List<S3ObjectMetadata>> mapReduceResult = mapResultEnumerator.Current;

					workConfig.FilesToProcess.Add (mapReduceResult.Key, mapReduceResult.Value);
				}
			}
		}

		public void Handle (ReduceWorkFinishedMessage message){
			Task task;
			Coordinator coordinator;
			if (tasks.TryGetValue(message.TaskId, out task) && task.ReduceCoordinators.TryGetValue (Sender, out coordinator)) {
				Console.WriteLine ("ReduceWorkFinished: " + message.TaskId + "-" + coordinator.Id + "-" + message.WorkerId);

				task.addReduceResult(message.File, message.Keys);

				if (coordinator.Works.ContainsKey (message.WorkerId)) {					
					coordinator.Works.Remove (message.WorkerId);
					Console.WriteLine (String.Format("Removed worker, count: {0} : {1}-{2}-{3}", coordinator.countWorksForTask(task.Id), task.Id, coordinator.Id, message.WorkerId));
				}
				else{Console.WriteLine (String.Format ("DIDNT Removed worker, count: {0} : {1}-{2}-{3}", coordinator.countWorksForTask(task.Id), task.Id, coordinator.Id, message.WorkerId));}

				if(coordinator.countWorksForTask(task.Id) == 0){					
					task.ReduceCoordinators.Remove (Sender);
					Console.WriteLine (String.Format("Removed coordinator, others: {0} : {1}-{2}-{3}", task.ReduceCoordinators.Count, task.Id, coordinator.Id, message.WorkerId));
				}

				//TODO: Co z ordered workami

				if(task.ReduceCoordinators.Count == 0){
					endTask (task);
				}
			} else {
				message.File.remove ();
				Console.WriteLine ("No coord found ReduceWorkFinishedMessage");
			}
		}

		void endTask (Task task)
		{
			Console.WriteLine ("End task: " + task.Id);
			task.deleteMapFiles ();

			TaskFinishedMessage taskFinishedMessage = new TaskFinishedMessage (){
				TaskId = task.Id,
				reduceResult = task.ReduceResult
			};

			tasks.Remove (task.Id);

			task.SS.Tell (taskFinishedMessage);

		}

		public void Handle (WorkerFailureMessage message)
		{
			Console.WriteLine (message.Message);

			Task task;
			if(tasks.TryGetValue(message.TaskId, out task)){
				tasks.Remove (message.TaskId);

				task.abort(message.Message);


			}
		}

		public void Handle (Terminated message)
		{
			Console.WriteLine("Coordinator terminated");
			IActorRef coordinatorActor = message.ActorRef;

			Coordinator mapCoord = validMapCoordinator.Find (coord => coord.CoordinatorActor.Equals(coordinatorActor));
			Coordinator reduceCoord = validReduceCoordinator.Find (coord => coord.CoordinatorActor.Equals(coordinatorActor));
			Coordinator coordinator;
			if (mapCoord != null) {
				coordinator = mapCoord;
				validMapCoordinator.Remove (coordinator);
			} else {
				coordinator = reduceCoord;
				validReduceCoordinator.Remove (coordinator);
			}

			foreach(Task task in tasks.Values){
				if (coordinator.IsMapCoordinator) {
					if (task.MapCoordinators.ContainsKey (coordinatorActor)) {
						task.MapCoordinators.Remove (coordinatorActor);
					}
				} else {
					if (task.ReduceCoordinators.ContainsKey (coordinatorActor)) {
						task.ReduceCoordinators.Remove (coordinatorActor);
					}
				}
			}

			foreach(Work work in coordinator.Works.Values){
				if (coordinator.IsMapCoordinator) {
					startNewMapWork (tasks[work.WorkConfig.TaskId], work.WorkConfig);
				} else {
					startNewReduceWork (tasks[work.WorkConfig.TaskId], work.WorkConfig);
				}
			}

			foreach(WorkConfig workConfig in coordinator.OrderedWorks.Values){
				if (coordinator.IsMapCoordinator) {
					startNewMapWork (tasks[workConfig.TaskId], workConfig);
				} else {
					startNewReduceWork (tasks[workConfig.TaskId], workConfig);
				}

			}
		}
			
		private Coordinator getNextMapCoordinator(){
			mapCoordinatorsRoundRobinIndex = (++mapCoordinatorsRoundRobinIndex) % validMapCoordinator.Count;
			//Console.WriteLine ("Next map coordinator: " + mapCoordinatorsRoundRobinIndex + " : " + validMapCoordinator.Count);
			return validMapCoordinator[mapCoordinatorsRoundRobinIndex];
		}

		private Coordinator getNextReduceCoordinator(){
			reduceCoordinatorsRoundRobinIndex = (++reduceCoordinatorsRoundRobinIndex) % validReduceCoordinator.Count;
			return validReduceCoordinator[reduceCoordinatorsRoundRobinIndex];
		}
	}
}

