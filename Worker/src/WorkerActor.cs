using System;
using Akka.Actor;
using MapReduceDotNetLib;

namespace Worker
{
	public abstract class WorkerActor : TypedActor, IHandle<NewWorkerMessage>
	{
		public WorkerActor ()
		{
		}

		public abstract void Handle (NewWorkerMessage message);
	}
}

