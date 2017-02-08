using System;
using Akka.Configuration;
using Akka.Actor;

namespace Worker
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var config = ConfigurationFactory.ParseString(@"
				akka {  
					actor {
						provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
					}
					remote {
						helios.tcp {
							port = 0
						}
					}
				}
			");

			var clientSystem = ActorSystem.Create("WorkerSystem", config);
			var workerActor = clientSystem.ActorOf<TestActor>("WorkerActor");
			workerActor.Tell (new object());

			Console.ReadLine ();
		}
	}
}
