using System;
using Akka.Actor;
using MapReduceDotNetLib;

namespace Worker
{
	public class CoordinatorActor : TypedActor
	{
		public CoordinatorActor ()
		{
			ActorSelection masterActorRef = getMasterActorRef ();

			masterActorRef.Tell (new RegisterMapWorkerMessage ());
		}

		ActorSelection getMasterActorRef ()
		{
			string masterAddress = Environment.GetEnvironmentVariable ("MASTER_ADDRESS");
			if (masterAddress == null) {
				masterAddress = "localhost:8081";
				Console.WriteLine ("No MASTER_ADDRESS found.");
			}

			Console.WriteLine ("MASTER_ADDRESS {0}", masterAddress);

			return Context.ActorSelection("akka.tcp://MasterSystem@" + masterAddress + "/user/MasterActor");
		}
	}
}

