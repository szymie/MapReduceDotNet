using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;

namespace Worker
{
	public class CoordinatorMapActor : CoordinatorActor, IHandle<MapWorkFinishedMessage>
	{
		public CoordinatorMapActor(){
			MasterActor.Tell (new RegisterMapCoordinatorMessage ());
		}

		public void Handle (MapWorkFinishedMessage message){
			if (workers.ContainsKey (Sender)) {
				workers.Remove (Sender);
				MasterActor.Tell (message);
			} else {
				foreach (KeyValuePair<string, S3ObjectMetadata> pair in message.MapResult) {
					pair.Value.remove ();
				}
			}

			Context.Stop (Sender);
		}

		protected override IActorRef createWorkerActor (int workerId)
		{
			Console.WriteLine ("Creating new map actor: " + this.CoordinatorId);
			return Context.System.ActorOf<MapActor> ("mapWorker" + workerId);
		}
	}
}

