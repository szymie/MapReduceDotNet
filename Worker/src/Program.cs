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
			//ConfigurationManager.AppSettings.Get
			//remoteActorTest ();
			executeAssemblyTest();
		}

		public static void executeAssemblyTest(){
			Assembly assembly = Assembly.LoadFrom("ClientLib.dll");
			Type type = assembly.GetType("ClientLib.MyWorker");
			MapReduceDotNetLib.Worker clientWorker =  (MapReduceDotNetLib.Worker) Activator.CreateInstance(type);
			clientWorker.map ("fileName", "fileContent2");
		}

		public static void remoteActorTest(){
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
