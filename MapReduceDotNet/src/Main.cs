using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Akka;
using Akka.Actor;

namespace MapReduceDotNet
{
	public class Msg1
	{
		public Msg1(string who)
		{
			Who = who;
		}

		public string Who { get; private set; }
	}

	public class Msg2
	{
		public Msg2(string who)
		{
			Who = who;
		}

		public string Who { get; private set; }
	}

	public class GreetingActor : TypedActor, IHandle<Msg1>, IHandle<Msg2>
	{
		public GreetingActor()
		{
			//Receive<Greet>(greet => Console.WriteLine("Hello {0}", greet.Who));
		}

		public void Handle(Msg2 message)
		{
			Console.WriteLine("Witam {0}", message.Who);
		}

		public void Handle(Msg1 message)
		{
			Console.WriteLine("Zegnam {0}", message.Who);
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			// Create a new actor system (a container for your actors)
			var system = ActorSystem.Create("MySystem");

			// Create your actor and get a reference to it.
			// This will be an "ActorRef", which is not a
			// reference to the actual actor instance
			// but rather a client or proxy to it.
			var greeter = system.ActorOf<GreetingActor>("greeter");

			// Send a message to the actor
			greeter.Tell(new Msg1("message1"));
			greeter.Tell(new Msg2("message2"));

			// This prevents the app from exiting
			// before the async work is done
			Console.ReadLine();
		}
	}
}
