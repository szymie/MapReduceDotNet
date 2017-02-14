using System;
using Akka.Configuration;
using Akka.Actor;
using System.Reflection;
using System.Configuration;
using System.IO;
using System.Threading;

namespace Worker
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			ActorSystem system = createActorSystem ("WorkerSystem");
			system.ActorOf<CoordinatorMapActor>("CoordinatorMapActor");
			system.ActorOf<CoordinatorReduceActor>("CoordinatorReduceActor");

			ManualResetEvent resetEvent = new ManualResetEvent(false);
			resetEvent.WaitOne();
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
	}
}
