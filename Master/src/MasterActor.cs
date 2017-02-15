using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Master
{
	public class MasterActor : TypedActor, IHandle<RegisterMapCoordinatorMessage>, IHandle<NewWorkAckMessage>, IHandle<Terminated>, IHandle<MapWorkFinishedMessage>, IHandle<WorkerFailureMessage>,
	IHandle<RegisterReduceCoordinatorMessage>, IHandle<NewTaskMessage>, IHandle<DivideResponseMessage>, IHandle<SortCoordinatorsByCpuUsage>
	{
		private List<Coordinator> validMapCoordinator = new List<Coordinator>();
		private List<Coordinator> validReduceCoordinator = new List<Coordinator>();

		private Dictionary<int, Task> tasks = new Dictionary<int, Task> ();

		private UniqueKeyGenerator coordinatorKeyGenerator = new UniqueKeyGenerator();

		private IActorRef fileDivider = Context.System.ActorOf<DividerActor>("dividerActor");

		public MasterActor(){
			Context.System.Scheduler.ScheduleTellRepeatedly (new TimeSpan(0, 0, 1), new TimeSpan(0,0,1), Self, new SortCoordinatorsByCpuUsage(), Self);
		}

		public void Handle(CoordinatorSystemInfo message){
			Coordinator coordinator;
			coordinator = validMapCoordinator.Find (coord => coord.Id == message.CoordinatorId);
			if(coordinator == null){
				coordinator = validReduceCoordinator.Find (coord => coord.Id == message.CoordinatorId);
			}

			if(coordinator != null){
				coordinator.CpuUsage = message.ProcessPercent;
			}
		}

		public void Handle(SortCoordinatorsByCpuUsage m){
			validMapCoordinator = validMapCoordinator.OrderBy(c=>c.CpuUsage).ToList();
			validReduceCoordinator = validReduceCoordinator.OrderBy(c=>c.CpuUsage).ToList();
		}

		public void Handle (RegisterMapCoordinatorMessage message){
			int coordinatorId = coordinatorKeyGenerator.generateKey ();
			Coordinator coordinator = new Coordinator (coordinatorId, Sender, true);

			validMapCoordinator.Add (coordinator);
			Context.Watch (Sender);
			Console.WriteLine ("Map coordinator registered with id: " + coordinatorId);
			Sender.Tell (new RegisterCoordinatorAckMessage(coordinatorId));
		}

		public void Handle (RegisterReduceCoordinatorMessage message){
			int coordinatorId = coordinatorKeyGenerator.generateKey ();
			Coordinator coordinator = new Coordinator (coordinatorId, Sender, false);

			validReduceCoordinator.Add (coordinator);
			Context.Watch (Sender);
			Console.WriteLine ("Reduce coordinator registered with id: " + coordinatorId);
			Sender.Tell (new RegisterCoordinatorAckMessage(coordinatorId));
		}

		public void Handle (NewTaskMessage message){
			if (validMapCoordinator.Count == 0 || validReduceCoordinator.Count == 0) {
				Sender.Tell (new TaskFailureMessage(message.TaskId, "No coordinators"));
				return;
			}

			Console.WriteLine ("Got new task: " + message.TaskId);
			tasks.Add (message.TaskId, new Task(message, Sender));

			fileDivider.Tell(new DivideRequestMessage(message.M, message.InputFiles, message.TaskId, message.Username));
		}

		public void Handle(DivideResponseMessage message){
			Task task = null;

			if (tasks.TryGetValue (message.TaskId, out task)) {
				if(message.Files.Count == 0){
					tasks.Remove (task.Id);
					task.abort ("No files to map");
					return;
				}

				task.addDivideFiles (message.Files);

				NextCoordinatorInBulkGetter coordinatorGetter = null;
				try {
					coordinatorGetter = new NextCoordinatorInBulkGetter (validMapCoordinator);
				} catch (Exception e) {
					tasks.Remove (task.Id);
					task.abort ("No coordinators");
					return;
				}
				foreach (Dictionary<string, S3ObjectMetadata> files in message.Files) {
					Dictionary<string, List<S3ObjectMetadata>> workConfigFiles = new Dictionary<string, List<S3ObjectMetadata>> ();

					foreach (KeyValuePair<string, S3ObjectMetadata> pair in files) {
						workConfigFiles.Add (pair.Key, new List<S3ObjectMetadata> (){ pair.Value });
					}

					WorkConfig workConfig = new WorkConfig (
						                        task.Id,
						                        task.Username,
						                        workConfigFiles,
						                        task.AssemblyMetadata
					                        );

					startNewMapWork (task, workConfig, coordinatorGetter.next ());
				}
			} else {
				foreach(Dictionary<string, S3ObjectMetadata> dict in message.Files){
					foreach(S3ObjectMetadata file in dict.Values){
						file.remove ();
					}
				}
			}
		}

		private void startNewMapWork(Task task, WorkConfig workConfig, Coordinator coordinator){
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
				} else {
					//Console.WriteLine ("No coord found NewWorkAckMessage");
				}
			} else {
				foreach (Coordinator mapCoordinator in validMapCoordinator) {
					if (mapCoordinator.OrderedWorks.ContainsKey (orderedWorkId)) {
						WorkConfig orderedWorkConfig = mapCoordinator.OrderedWorks [orderedWorkId];
						Sender.Tell (new AbortWorkMessage(message.WorkerId));

						mapCoordinator.OrderedWorks.Remove (orderedWorkId);
					}
				}

				foreach (Coordinator reduceCoordinator in validReduceCoordinator) {
					if (reduceCoordinator.OrderedWorks.ContainsKey (orderedWorkId)) {
						WorkConfig orderedWorkConfig = reduceCoordinator.OrderedWorks [orderedWorkId];
						Sender.Tell (new AbortWorkMessage(message.WorkerId));

						reduceCoordinator.OrderedWorks.Remove (orderedWorkId);
					}
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
					//Console.WriteLine ("No coord found MapWorkFinishedMessage");
					
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

			if(reduceWorkConfigs.Count == 0){
				tasks.Remove (task.Id);
				task.abort ("No files to reduce");
				return;
			}

			NextCoordinatorInBulkGetter coordinatorGetter = null;
			try{
				coordinatorGetter= new NextCoordinatorInBulkGetter (validReduceCoordinator);
			}
			catch(Exception e){
				tasks.Remove (task.Id);
				task.abort ("No coordinator");
				return;
			}

			foreach (var workConfig in reduceWorkConfigs) {
				startNewReduceWork (task, workConfig, coordinatorGetter.next());
			}
		}

		private void startNewReduceWork(Task task, WorkConfig workConfig, Coordinator coordinator){			
			Console.WriteLine ("started reduce work: " + task.Id + "-" + coordinator.Id);

			int orderedWorkConfigId = coordinator.storeOrderedWork (workConfig);

			NewWorkMessage newWorkMessage = new NewWorkMessage (orderedWorkConfigId, workConfig);

			if(!task.ReduceCoordinators.ContainsKey(coordinator.CoordinatorActor)){
				task.ReduceCoordinators.Add(coordinator.CoordinatorActor, coordinator);
			}

			coordinator.CoordinatorActor.Tell (newWorkMessage);
		}

		private List<WorkConfig> createReduceConfigs(Task task){
			int reduceConfigNum = Math.Min (task.R, task.MapResult.Count);

			List<WorkConfig> reduceWorkConfigs = new List<WorkConfig> (reduceConfigNum);

			for (int i = 0; i < reduceConfigNum; i++) {
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
					//Console.WriteLine (String.Format("Removed worker, count: {0} : {1}-{2}-{3}", coordinator.countWorksForTask(task.Id), task.Id, coordinator.Id, message.WorkerId));
				}
				//else{Console.WriteLine (String.Format ("DIDNT Removed worker, count: {0} : {1}-{2}-{3}", coordinator.countWorksForTask(task.Id), task.Id, coordinator.Id, message.WorkerId));}

				if(coordinator.countWorksForTask(task.Id) == 0){					
					task.ReduceCoordinators.Remove (Sender);
					//Console.WriteLine (String.Format("Removed coordinator, others: {0} : {1}-{2}-{3}", task.ReduceCoordinators.Count, task.Id, coordinator.Id, message.WorkerId));
				}
					
				if(task.ReduceCoordinators.Count == 0){
					endTask (task);
				}
			} else {
				message.File.remove ();
				//Console.WriteLine ("No coord found ReduceWorkFinishedMessage");
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
			Task task;
			if(tasks.TryGetValue(message.TaskId, out task)){
				tasks.Remove (message.TaskId);

				Console.WriteLine ("Aborting task: " + message.Message);
				task.abort(message.Message);
			}
		}

		public void Handle (Terminated message)
		{
			Console.WriteLine("Coordinator terminated");
			IActorRef coordinatorActor = message.ActorRef;

			Coordinator mapCoord = validMapCoordinator.Find (coord => coord.CoordinatorActor.Equals(coordinatorActor));
			Coordinator reduceCoord = validReduceCoordinator.Find (coord => coord.CoordinatorActor.Equals(coordinatorActor));
			Coordinator coordinator = null;
			NextCoordinatorInBulkGetter coordinatorGetter = null;
			try{
				if (mapCoord != null) {
					coordinator = mapCoord;
					validMapCoordinator.Remove (coordinator);
					coordinatorGetter = new NextCoordinatorInBulkGetter (validMapCoordinator);
				} else {
					coordinator = reduceCoord;
					validReduceCoordinator.Remove (coordinator);
					coordinatorGetter = new NextCoordinatorInBulkGetter (validReduceCoordinator);
				}
			}
			catch(Exception e){}

			foreach(int taskId in tasks.Keys.ToList()){
				Task task = tasks [taskId];
				if (coordinatorGetter == null) {
					tasks.Remove (taskId);
					task.abort ("No coordinators");
					continue;
				}

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

			if (coordinatorGetter == null) {
				return;
			}

			foreach(Work work in coordinator.Works.Values){
				if (coordinator.IsMapCoordinator) {
					startNewMapWork (tasks[work.WorkConfig.TaskId], work.WorkConfig, coordinatorGetter.next());
				} else {
					startNewReduceWork (tasks[work.WorkConfig.TaskId], work.WorkConfig, coordinatorGetter.next());
				}
			}

			foreach(WorkConfig workConfig in coordinator.OrderedWorks.Values){
				if (coordinator.IsMapCoordinator) {
					startNewMapWork (tasks[workConfig.TaskId], workConfig, coordinatorGetter.next());
				} else {
					startNewReduceWork (tasks[workConfig.TaskId], workConfig, coordinatorGetter.next());
				}

			}
		}
	}
}

