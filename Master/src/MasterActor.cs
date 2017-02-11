using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;

namespace Master
{
	public class MasterActor : TypedActor, IHandle<RegisterMapCoordinatorMessage>, IHandle<Terminated>, IHandle<MapWorkFinishedMessage>
	{

		public void Handle (RegisterMapCoordinatorMessage message)
		{
			Context.Watch (Sender);
			throw new NotImplementedException ();
		}

		public void Handle (MapWorkFinishedMessage message)
		{
			//Context.Watch (Sender);
			throw new NotImplementedException ();
		}

		public void Handle (Terminated message)
		{
			string disconnectedActorPath = message.ActorRef.Path.ToString();
			throw new NotImplementedException ();
		}
			
		public MasterActor ()
		{			

		}
	}
}

