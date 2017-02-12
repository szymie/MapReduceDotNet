using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.IO;

namespace Master
{
	public class MasterActor : TypedActor, IHandle<RegisterMapCoordinatorMessage>, IHandle<NewWorkAckMessage>, IHandle<Terminated>, IHandle<MapWorkFinishedMessage>, IHandle<WorkerFailureMessage>
	{

		public void Handle (RegisterMapCoordinatorMessage message)
		{
			//Context.Watch (Sender);
			//throw new NotImplementedException ();
			Console.WriteLine("new register ");
			S3ObjectMetadata testData1 = new S3ObjectMetadata ("testBucket", "/tmp/testData1.txt");
			S3ObjectMetadata testData2 = new S3ObjectMetadata ("testBucket", "/tmp/testData2.txt");
			AssemblyMetadata assemblyMetadata = new AssemblyMetadata ("ClientLib", "MyWorker", "testReduceClassName", new S3ObjectMetadata("testBucket", "/home/gemboj/Polibuda/sem2/piksr/MapReduceDotNet/ClientLib/bin/Debug/ClientLib.dll"));


			Dictionary<string, S3ObjectMetadata> filesToProcess1 = new Dictionary<string, S3ObjectMetadata> ();
			filesToProcess1.Add ("testData1.txt", testData1);

			Dictionary<string, S3ObjectMetadata> filesToProcess2 = new Dictionary<string, S3ObjectMetadata> ();
			filesToProcess2.Add ("testData2.txt", testData2);

			Sender.Tell (new RegisterCoordinatorAckMessage(0));
			Sender.Tell (new NewWorkMessage(0, new WorkConfig(0, "testUsername", filesToProcess1, assemblyMetadata)));
			//Sender.Tell (new NewWorkMessage(1, new WorkConfig(0, "testUsername", filesToProcess2, assemblyMetadata)));
		}

		public void Handle (NewWorkAckMessage message){
			Console.WriteLine ("workAckMessage: " + message.WorkId);
		}

		public void Handle (MapWorkFinishedMessage message)
		{
			Console.WriteLine ("Work finished: ");
			foreach(KeyValuePair<string, S3ObjectMetadata> pair in message.MapResult){
				var filename = "/tmp/" + pair.Value.Filename;
				if (File.Exists (filename)) {
					Console.WriteLine ("exists: " + filename);
				} else {
					Console.WriteLine ("NOT: " + filename);
				}
			}

			//Context.Watch (Sender);
			//throw new NotImplementedException ();
		}

		public void Handle (WorkerFailureMessage message)
		{
			Console.WriteLine ("worker failure: " + message.Message);
		}

		public void Handle (Terminated message)
		{
			//string disconnectedActorPath = message.ActorRef.Path.ToString();
			throw new NotImplementedException ();
		}
			
		public MasterActor ()
		{			

		}
	}
}

