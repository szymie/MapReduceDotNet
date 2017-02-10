using System;
using Akka.Configuration;
using Akka.Actor;
using System.Reflection;
using System.Configuration;

namespace Worker
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			ActorSystem system = createActorSystem ("WorkerSystem");
			var workerActor = system.ActorOf<TestActor>("WorkerActor");
			workerActor.Tell(new Object());

			Console.ReadLine ();
			//executeAssemblyTest();
		}

		static ActorSystem createActorSystem (string systemName)
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

			return ActorSystem.Create(systemName, config);
		}

		public static void executeAssemblyTest(){
			Assembly assembly = Assembly.LoadFrom("ClientLib.dll");
			Type type = assembly.GetType("ClientLib.MyWorker");
			MapReduceDotNetLib.Worker clientWorker = (MapReduceDotNetLib.Worker) Activator.CreateInstance(type);
			clientWorker.map ("fileName", "fileContent2");
		}
	}
}
