using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;

namespace Master
{
	public class MasterActor : TypedActor, IHandle<RegisterMapWorkerMessage>, IHandle<Terminated>
	{

		public void Handle (RegisterMapWorkerMessage message)
		{
			Context.Watch (Sender);
		}

		public void Handle (Terminated message)
		{
			string disconnectedActorPath = message.ActorRef.Path.ToString();

		}
			
		public MasterActor ()
		{			

		}
	}
}

