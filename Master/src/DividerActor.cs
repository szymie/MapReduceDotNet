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

			/*foreach (var singleFile in result)
			{
				foreach (var pair in singleFile)
				{
					Console.WriteLine(pair.Key);
					Console.WriteLine(pair.Value.Filename);
				}

				Console.WriteLine("---");
			}*/

			var response = new DivideResponseMessage(result, message.TaskId);
			Sender.Tell(response);
		}
	}
}
