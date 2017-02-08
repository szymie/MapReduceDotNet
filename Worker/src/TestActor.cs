using System;
using Akka.Actor;
using MapReduceDotNetLib;

namespace Worker
{
	public class TestActor : TypedActor, IHandle<Object>,  IHandle<string>
	{
		//private readonly ActorSelection masterActor = Context.ActorSelection("akka.tcp://MasterSystem@localhost:8081/user/MasterActor");

		public void Handle (object message)
		{
			string masterAddress = Environment.GetEnvironmentVariable ("MASTER_ADDRESS");
			if (masterAddress == null) {
				masterAddress = "localhost:8081";
				Console.WriteLine ("No MASTER_ADDRESS found.");
			}

			Console.WriteLine ("MASTER_ADDRESS {0}", masterAddress);

			var masterActor = Context.ActorSelection("akka.tcp://MasterSystem@" + masterAddress + "/user/MasterActor");

			Console.Write ("sending...");
			masterActor.Tell (new RegisterMessage ("test"));
		}

		public void Handle (string message){
			Console.WriteLine ("Received: {0}", message);
		}
			
		public TestActor ()
		{
			
		}
	}
}

