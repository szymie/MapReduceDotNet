using System;
using Akka.Actor;

namespace Master
{
	public class DividerActor : TypedActor, IHandle<DivideRequestMessage>
	{
		public void Handle(DivideRequestMessage message)
		{
			var divider = new Divider(message.Files, message.M, message.TaskId, message.Username);
			var result = divider.divide();
			var response = new DivideResponseMessage(result, message.TaskId);
			Sender.Tell(response);
		}
	}
}
