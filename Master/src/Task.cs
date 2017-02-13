using System;
using System.Collections.Generic;
using Akka.Actor;
using MapReduceDotNetLib;

namespace Master
{
	public class Task
	{
		public int Id{get;set;}
		public int M{ get; set; }
		public int R{ get; set; }
		public Dictionary<string, S3ObjectMetadata> InputFiles{ get; set; }
		public AssemblyMetadata AssemblyMetadata{ get; set; }
		public string Username{ get; set; }
		public IActorRef SS{ get; set; }


		public Dictionary<IActorRef, Coordinator> MapCoordinators{ get; set; } = new Dictionary<IActorRef, Coordinator>();
		public Dictionary<IActorRef, Coordinator> ReduceCoordinators{ get; set; } = new Dictionary<IActorRef, Coordinator>();

		public List<S3ObjectMetadata> DivideFiles{ get; set;} = new List<S3ObjectMetadata>();
		public List<S3ObjectMetadata> MapFiles{ get; set;} = new List<S3ObjectMetadata>();
		public List<S3ObjectMetadata> ReduceFiles{ get; set;} = new List<S3ObjectMetadata>();

		public Dictionary<string, List<S3ObjectMetadata>> MapResult{ get; set; } = new Dictionary<string, List<S3ObjectMetadata>>();
		public List<Tuple<S3ObjectMetadata, List<string>>> ReduceResult{ get; set; } = new List<Tuple<S3ObjectMetadata, List<string>>>(); //gdy widzisz liste< par <obiektow i list>>, to wiedz, ze cos sie dzieje



		public Task (NewTaskMessage newTaskMessage, IActorRef ss)
		{
			this.Id = newTaskMessage.TaskId;
			this.M = newTaskMessage.M;
			this.R = newTaskMessage.R;
			this.InputFiles = newTaskMessage.InputFiles;
			this.AssemblyMetadata = newTaskMessage.Assembly;
			this.Username = newTaskMessage.Username;
			this.SS = ss;
		}

		public void addDivideFiles (List<Dictionary<string, S3ObjectMetadata>> files)
		{
			foreach(Dictionary<string, S3ObjectMetadata> fileMap in files){
				foreach(S3ObjectMetadata s3ObjectMetadata in fileMap.Values){
					DivideFiles.Add (s3ObjectMetadata);
				}
			}
		}

		public void addMapResult (Dictionary<string, S3ObjectMetadata> mapResult)
		{
			foreach (KeyValuePair<string, S3ObjectMetadata> pair in mapResult) {
				List<S3ObjectMetadata> keyFiles;
				if (MapResult.TryGetValue (pair.Key, out keyFiles)) {
					MapResult.Remove (pair.Key);
					keyFiles.Add (pair.Value);

					MapResult.Add (pair.Key, keyFiles);
					MapFiles.Add (pair.Value);
				} else {
					mapResult.Add (pair.Key, pair.Value);
					MapFiles.Add (pair.Value);
				}
			}

			throw new NotImplementedException ();
		}

		public void addReduceResult (S3ObjectMetadata file, List<string> keys)
		{
			ReduceResult.Add (new Tuple<S3ObjectMetadata, List<string>>(file, keys));
			ReduceFiles.Add (file);
		}

		public bool getCoordinatorByActorRef(IActorRef coordinatorActor, out Coordinator coordinator, out bool isMapCoordinator){
			if (MapCoordinators.TryGetValue (coordinatorActor, out coordinator)){
				isMapCoordinator = true;
				return true;
			}

			if (ReduceCoordinators.TryGetValue (coordinatorActor, out coordinator)) {
				isMapCoordinator = false;
				return true;
			}

			isMapCoordinator = false;
			return false;
		}

		public void abort (string message){
			foreach(Coordinator coordinator in MapCoordinators.Values){
				coordinator.abort (Id);
			}

			foreach(Coordinator coordinator in MapCoordinators.Values){
				coordinator.abort (Id);
			}


			deleteDividerFiles ();
			deleteMapFiles ();
			deleteReduceFiles ();


			SS.Tell (new TaskFailureMessage (Id, message));
		}

		public void deleteDividerFiles(){
			foreach(S3ObjectMetadata s3ObjectMetadata in DivideFiles){
				s3ObjectMetadata.remove ();
			}
		}

		public void deleteMapFiles(){
			foreach(S3ObjectMetadata s3ObjectMetadata in MapFiles){
				s3ObjectMetadata.remove ();
			}
		}

		public void deleteReduceFiles(){
			foreach(S3ObjectMetadata s3ObjectMetadata in ReduceFiles){
				s3ObjectMetadata.remove ();
			}
		}
	}
}

