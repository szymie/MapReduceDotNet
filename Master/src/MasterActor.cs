using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.IO;

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
		private UniqueKeyGenerator taskKeyGenerator = new UniqueKeyGenerator();

		private IActorRef fileDivider;

		public void Handle (RegisterMapCoordinatorMessage message){
			int coordinatorId = coordinatorKeyGenerator.generateKey ();
			Coordinator coordinator = new Coordinator (coordinatorId, Sender);

			validMapCoordinator.Add (coordinator);
			Sender.Tell (new RegisterCoordinatorAckMessage(coordinatorId));
		}

		public void Handle (RegisterReduceCoordinatorMessage message){
			int coordinatorId = coordinatorKeyGenerator.generateKey ();
			Coordinator coordinator = new Coordinator (coordinatorId, Sender);

			validReduceCoordinator.Add (coordinator);
			Sender.Tell (new RegisterCoordinatorAckMessage(coordinatorId));
		}

		public void Handle (NewTaskMessage message){
			int taskId = taskKeyGenerator.generateKey ();
			tasks.Add (taskId, new Task(message, Sender));

			fileDivider.Tell(new DivideRequestMessage (message.M, message.InputFiles, taskId));
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

			if(!task.MapCoordinators.ContainsKey(coordinator.CoordinatorActor)){
				task.MapCoordinators.Add(coordinator.CoordinatorActor, coordinator);
			}

			int orderedWorkConfigId = coordinator.storeOrderedWork (workConfig);

			coordinator.CoordinatorActor.Tell (new NewWorkMessage (orderedWorkConfigId, workConfig));
		}

		public void Handle (NewWorkAckMessage message){
			int orderedWorkId = message.OrderedWorkId;
			Task task;
			if(tasks.TryGetValue(message.TaskId, out task)){
				Coordinator coordinator;
				bool isMapCoordinator;
				if (task.getCoordinatorByActorRef (Sender, out coordinator, out isMapCoordinator)) {
					WorkConfig orderedWorkConfig = coordinator.OrderedWorks[orderedWorkId];
					coordinator.OrderedWorks.Remove (orderedWorkId);

					Work work = new Work (task.Id, message.WorkerId, orderedWorkConfig);
					coordinator.Works.Add (message.WorkerId, work);
				}
			}
		}

		public void Handle (MapWorkFinishedMessage message){
			Task task;
			Coordinator coordinator;
			if (tasks.TryGetValue (message.TaskId, out task) && task.MapCoordinators.TryGetValue (Sender, out coordinator)) {

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
			Coordinator coordinator = getNextMapCoordinator ();

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

				task.addReduceResult(message.File, message.Keys);

				if (coordinator.Works.ContainsKey (message.WorkerId)) {
					coordinator.Works.Remove (message.WorkerId);
				}

				if(coordinator.countWorksForTask(task.Id) == 0){
					task.ReduceCoordinators.Remove (Sender);
				}

				if(task.ReduceCoordinators.Count == 0){
					endTask (task);
				}
			} else {
				message.File.remove ();
			}
		}

		void endTask (Task task)
		{
			task.deleteMapFiles ();

			TaskFinishedMessage taskFinishedMessage = new TaskFinishedMessage (){
				TaskId = task.Id,
				reduceResult = null//task.ReduceResult
			};

			tasks.Remove (task.Id);

			task.SS.Tell (taskFinishedMessage);

			throw new NotImplementedException ("null w TaskFinishedMessage");
		}

		public void Handle (WorkerFailureMessage message)
		{
			Task task;
			if(tasks.TryGetValue(message.TaskId, out task)){
				tasks.Remove (message.TaskId);

				task.abort(message.Message);
			}
		}

		public void Handle (Terminated message)
		{
			IActorRef coordinatorActor = message.ActorRef;
			foreach (Coordinator coordinator in validMapCoordinator) {
				if (coordinator.CoordinatorActor.Equals (coordinatorActor)) {
					validMapCoordinator.Remove (coordinator);
				}
			}

			foreach (Coordinator coordinator in validReduceCoordinator) {
				if (coordinator.CoordinatorActor.Equals (coordinatorActor)) {
					validReduceCoordinator.Remove (coordinator);
				}
			}

			foreach(Task task in tasks.Values){
				Coordinator coordinator;
				bool isMapCoordinator;
				if (task.getCoordinatorByActorRef (coordinatorActor, out coordinator, out isMapCoordinator)) {
					if (isMapCoordinator) {
						task.MapCoordinators.Remove (coordinatorActor);
					} else {
						task.ReduceCoordinators.Remove (coordinatorActor);
					}

					foreach(Work work in coordinator.Works.Values){
						if (isMapCoordinator) {
							startNewMapWork (task, work.WorkConfig);
						} else {
							startNewReduceWork (task, work.WorkConfig);
						}
					}

					foreach(WorkConfig workConfig in coordinator.OrderedWorks.Values){
						if (isMapCoordinator) {
							startNewMapWork (task, workConfig);
						} else {
							startNewReduceWork (task, workConfig);
						}
					}
				}
			}
		}
			
		private Coordinator getNextMapCoordinator(){
			mapCoordinatorsRoundRobinIndex = (mapCoordinatorsRoundRobinIndex++) % validMapCoordinator.Count;
			return validMapCoordinator[mapCoordinatorsRoundRobinIndex];
		}

		private Coordinator getNextReduceCoordinator(){
			reduceCoordinatorsRoundRobinIndex = (reduceCoordinatorsRoundRobinIndex++) % validReduceCoordinator.Count;
			return validReduceCoordinator[reduceCoordinatorsRoundRobinIndex];
		}
	}
}

