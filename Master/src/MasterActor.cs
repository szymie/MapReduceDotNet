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
		private Dictionary<int, NewTaskMessage> NewTaskData = new Dictionary<int, NewTaskMessage> ();

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
			NewTaskData.Add (taskId, message);

			fileDivider.Tell(new DivideRequestMessage (message.M, message.Files, taskId));
		}

		public void Handle(DivideResponseMessage message){
			Task task = new Task (message.TaskId);
			NewTaskMessage newTaskMessage = NewTaskData [message.TaskId];

			foreach(Dictionary<string, S3ObjectMetadata> files in message.Files){
				Coordinator coordinator = getNextMapCoordinator ();

				WorkConfig workConfig = new WorkConfig (
					task.Id,
					newTaskMessage.Username,
					files,
					newTaskMessage.Assembly,
					""
				);

				int orderedWorkConfig = coordinator.storeOrderedWork (workConfig);
				NewWorkMessage newWorkMessage = new NewWorkMessage (orderedWorkConfig, workConfig);
				coordinator.CoordinatorActor.Tell (newWorkMessage);

				if(task.MapCoordinators.ContainsKey(coordinator.Id)){

				}
			}
		}

		public void Handle (NewWorkAckMessage message){
			int orderedWorkId = message.OrderedWorkId;
			Task task = tasks [message.TaskId];
			Coordinator coordinator = task.getCoordinatorByActorRef (Sender);

			WorkConfig orderedWorkConfig = coordinator.OrderedWorks[orderedWorkId];
			coordinator.OrderedWorks.Remove (orderedWorkId);

			Work work = new Work (message.WorkerId, orderedWorkConfig);
			coordinator.Works.Add (message.WorkerId, work);
		}

		public void Handle (WorkerFailureMessage message)
		{
			throw new NotImplementedException ();
		}

		public void Handle (MapWorkFinishedMessage message){
			Task task = tasks [message.TaskId];
			Coordinator coordinator = task.MapCoordinators [Sender];

			coordinator.Works.Remove (message.WorkerId);
			coordinator.


			throw new NotImplementedException ();
		}

		public void Handle (ReduceWorkFinishedMessage message){
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

