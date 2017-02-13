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

			foreach(Dictionary<string, S3ObjectMetadata> files in message.Files){
				Dictionary<string, List<S3ObjectMetadata>> workConfigMap = new Dictionary<string, List<S3ObjectMetadata>> ();

				foreach(KeyValuePair<string, S3ObjectMetadata> pair in files){
					workConfigMap.Add (pair.Key, new List<S3ObjectMetadata> (){pair.Value});
				}

				Coordinator coordinator = getNextMapCoordinator ();

				WorkConfig workConfig = new WorkConfig (
					task.Id,
					task.Username,
					workConfigMap,
					task.AssemblyMetadata
				);

				int orderedWorkConfigId = coordinator.storeOrderedWork (workConfig);
				NewWorkMessage newWorkMessage = new NewWorkMessage (orderedWorkConfigId, workConfig);

				if(!task.MapCoordinators.ContainsKey(coordinator.CoordinatorActor)){
					task.MapCoordinators.Add(coordinator.CoordinatorActor, coordinator);
				}

				coordinator.CoordinatorActor.Tell (newWorkMessage);
			}
		}

		public void Handle (NewWorkAckMessage message){
			int orderedWorkId = message.OrderedWorkId;
			Task task = tasks [message.TaskId];
			Coordinator coordinator;
			bool isMapCoordinator;
			if (task.getCoordinatorByActorRef (Sender, out coordinator, out isMapCoordinator)) {
				WorkConfig orderedWorkConfig = coordinator.OrderedWorks[orderedWorkId];
				coordinator.OrderedWorks.Remove (orderedWorkId);

				Work work = new Work (message.WorkerId, orderedWorkConfig);
				coordinator.Works.Add (message.WorkerId, work);
			}
		}

		public void Handle (MapWorkFinishedMessage message){
			Task task = tasks [message.TaskId];
			Coordinator coordinator;
			if (task.MapCoordinators.TryGetValue (Sender, out coordinator)) {
				foreach(KeyValuePair<string, S3ObjectMetadata> pair in message.MapResult){
					List<S3ObjectMetadata> keyFiles;
					if (task.MapResult.TryGetValue (pair.Key, out keyFiles)) {
						task.MapResult.Remove (pair.Key);
						keyFiles.Add(pair.Value);

						task.MapResult.Add(pair.Key, keyFiles);
					}
				}

				if (coordinator.Works.ContainsKey (message.WorkerId)) {
					coordinator.Works.Remove (message.WorkerId);
				}

				if(coordinator.Works.Count == 0){
					task.MapCoordinators.Remove (Sender);
				}

				if(task.MapCoordinators.Count == 0){
					startReduceProcessing (task);
				}
			}
		}

		private void startReduceProcessing (Task task){
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
			Task task = tasks [message.TaskId];
			Coordinator coordinator;
			if (task.ReduceCoordinators.TryGetValue (Sender, out coordinator)) {
				task.ReduceResult.Add (new Tuple<S3ObjectMetadata, List<string>>(message.File, message.Keys));

				if (coordinator.Works.ContainsKey (message.WorkerId)) {
					coordinator.Works.Remove (message.WorkerId);
				}

				if(coordinator.Works.Count == 0){
					task.ReduceCoordinators.Remove (Sender);
				}

				if(task.ReduceCoordinators.Count == 0){
					endTask (task);
				}
			}
		}

		void endTask (Task task)
		{
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
			/*
			Task task;
			if(tasks.TryGetValue(message.TaskId, out task)){
				Coordinator coordinator;
				bool isMapCoordinator;
				if (task.getCoordinatorByActorRef (Sender, out coordinator, out isMapCoordinator)) {
					Work work;
					if(coordinator.Works.TryGetValue(message.WorkerId, out work)){
						//work.
					}
				}
			}*/
			throw new NotImplementedException ();
		}

		public void Handle (Terminated message)
		{
			throw new NotImplementedException ();
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

