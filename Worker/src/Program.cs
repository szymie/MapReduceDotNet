using System;
using Akka.Configuration;
using Akka.Actor;
using System.Reflection;
using System.Configuration;
using System.IO;
using System.Threading;
using MapReduceDotNetLib;

namespace Worker
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			RemoveMyFiles.removeFiles ();

			ActorSystem system = createActorSystem ("WorkerSystem");
			system.ActorOf<CoordinatorMapActor>("CoordinatorMapActor");
			system.ActorOf<CoordinatorReduceActor>("CoordinatorReduceActor");

			ManualResetEvent resetEvent = new ManualResetEvent(false);
			resetEvent.WaitOne();
		}

		static ActorSystem createActorSystem (string systemName)
		{

			string maximumFrameSize = Environment.GetEnvironmentVariable ("MAXIMUM_FRAME_SIZE");
			if (maximumFrameSize == null) {
				maximumFrameSize = "4000000b";
				Console.WriteLine ("No MAXIMUM_FRAME_SIZE found.");
			}

			var config = ConfigurationFactory.ParseString(String.Format(@"
				akka {{  
					actor {{
						provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
					}}
					remote {{
						helios.tcp {{
							port = 0
							maximum-frame-size = {0}
						}}
					}}
				}}
			", maximumFrameSize));

			Console.WriteLine ("maximum frame size: " + maximumFrameSize);

			return ActorSystem.Create(systemName, config);
		}
	}
}
