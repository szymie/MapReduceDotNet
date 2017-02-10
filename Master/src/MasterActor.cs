using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;

namespace Master
{
	public class TestActor : TypedActor, IHandle<RegisterMapWorkerMessage>, IHandle<Terminated>
	{
		List<IActorRef> registeredWorkers = new List<IActorRef>();

		public void Handle (RegisterMapWorkerMessage message)
		{
			if (registeredWorkers.Count != 0) {
				var actorRef = registeredWorkers [0];
			
				try {
					//actorRef.Tell("costam");
					//Context.Stop();
					actorRef.Tell(Kill.Instance);
					Console.WriteLine("Killed actor");

				} catch (Exception e) {
					Console.WriteLine ("Exception while sending.");
				}
			}

			Console.WriteLine ("Registered actor: {0}", Sender.Path);
			registeredWorkers.Add(Sender);
			Context.Watch (Sender);
		}

		public void Handle (Terminated message)
		{
			string disconnectedActorPath = message.ActorRef.Path.ToString();

			//Console.WriteLine ("Connection terminated: {0}", message.ToString());
			int disconnectedActorIndex = registeredWorkers.FindIndex(
				actorRef => actorRef.Path.ToString().Equals(disconnectedActorPath)
			);

			if (disconnectedActorIndex != -1) {
				registeredWorkers.RemoveAt (disconnectedActorIndex);
				Console.WriteLine ("Deregistered actor: {0}", disconnectedActorPath);
			} else {
				Console.WriteLine ("Actor not found: {0}", disconnectedActorPath);
			}



		}
			
		public TestActor ()
		{			
			//Receive<TestMessage>(message =>);
			Console.WriteLine("Waiting for messages...");
		}
	}
}

