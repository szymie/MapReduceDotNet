using System;
using Akka.Actor;
using MapReduceDotNetLib;

namespace Worker
{
	public abstract class WorkerActor : TypedActor, IHandle<WorkerConfig>
	{
		public WorkerActor ()
		{
		}

		public abstract void Handle (WorkerConfig message);
	}
}

