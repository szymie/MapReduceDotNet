using System;
using Akka.Actor;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.IO;

namespace Master
{
	public class MasterActor : TypedActor, IHandle<RegisterMapCoordinatorMessage>, IHandle<NewWorkAckMessage>, IHandle<Terminated>, IHandle<MapWorkFinishedMessage>, IHandle<WorkerFailureMessage>,
	IHandle<RegisterReduceCoordinatorMessage>
	{

		public void Handle (RegisterMapCoordinatorMessage message)
		{
			//Context.Watch (Sender);
			//throw new NotImplementedException ();
			Console.WriteLine("new map register ");
			S3ObjectMetadata testData1 = new S3ObjectMetadata ("testBucket", "/tmp/testData1");
			S3ObjectMetadata testData2 = new S3ObjectMetadata ("testBucket", "/tmp/testData2");
			S3ObjectMetadata testData3 = new S3ObjectMetadata ("testBucket", "/tmp/testData3");
			S3ObjectMetadata testData4 = new S3ObjectMetadata ("testBucket", "/tmp/testData4");
			AssemblyMetadata assemblyMetadata = new AssemblyMetadata ("ClientLib", "MyWorker", "MyReduce", new S3ObjectMetadata("testBucket", "/home/gemboj/Polibuda/sem2/piksr/MapReduceDotNet/ClientLib/bin/Debug/ClientLib.dll"));


			Dictionary<string, S3ObjectMetadata> filesToProcess1 = new Dictionary<string, S3ObjectMetadata> ();
			filesToProcess1.Add ("testData1", testData1);
			filesToProcess1.Add ("testData2", testData2);

			Dictionary<string, S3ObjectMetadata> filesToProcess2 = new Dictionary<string, S3ObjectMetadata> ();
			filesToProcess2.Add ("testData3", testData3);
			filesToProcess2.Add ("testData4", testData4);


			Sender.Tell (new RegisterCoordinatorAckMessage(0));
			Sender.Tell (new NewWorkMessage(0, new WorkConfig(0, "mapUsername", filesToProcess1, assemblyMetadata, "0")));
			Sender.Tell (new NewWorkMessage(1, new WorkConfig(0, "mapUsername", filesToProcess2, assemblyMetadata, "1")));
		}

		public void Handle (RegisterReduceCoordinatorMessage message){
			Console.WriteLine("new reduce register ");
			S3ObjectMetadata testData1 = new S3ObjectMetadata ("testBucket", "/tmp/testData1");
			S3ObjectMetadata testData2 = new S3ObjectMetadata ("testBucket", "/tmp/testData2");
			S3ObjectMetadata testData3 = new S3ObjectMetadata ("testBucket", "/tmp/testData3");
			S3ObjectMetadata testData4 = new S3ObjectMetadata ("testBucket", "/tmp/testData4");
			AssemblyMetadata assemblyMetadata = new AssemblyMetadata ("ClientLib", "MyWorker", "MyReduce", new S3ObjectMetadata("testBucket", "/home/gemboj/Polibuda/sem2/piksr/MapReduceDotNet/ClientLib/bin/Debug/ClientLib.dll"));


			Dictionary<string, S3ObjectMetadata> filesToProcess1 = new Dictionary<string, S3ObjectMetadata> ();
			filesToProcess1.Add ("testData1", testData1);
			filesToProcess1.Add ("testData2", testData2);

			Dictionary<string, S3ObjectMetadata> filesToProcess2 = new Dictionary<string, S3ObjectMetadata> ();
			filesToProcess2.Add ("testData3", testData3);
			filesToProcess2.Add ("testData4", testData4);

			Sender.Tell (new RegisterCoordinatorAckMessage(1));
			Sender.Tell (new NewWorkMessage (2, new WorkConfig (0, "reduceUsername", filesToProcess1, assemblyMetadata, "someKey")));
			Sender.Tell (new NewWorkMessage (3, new WorkConfig (0, "reduceUsername", filesToProcess2, assemblyMetadata, "someKey")));
		}

		public void Handle (NewWorkAckMessage message){
			Console.WriteLine ("workAckMessage: " + message.WorkId);
		}

		public void Handle (MapWorkFinishedMessage message)
		{
			Console.WriteLine ("Map Work finished: ");
			foreach(KeyValuePair<string, S3ObjectMetadata> pair in message.MapResult){
				var filename = "/tmp/" + pair.Value.Filename;
				if (File.Exists (filename)) {
					Console.WriteLine ("exists: " + filename);
				} else {
					Console.WriteLine ("NOT: " + filename);
				}
			}
		}

		public void Handle (ReduceWorkFinishedMessage message){
			Console.WriteLine ("Reduce Work finished: ");

			var filename = "/tmp/" + message.File.Filename;
			if (File.Exists (filename)) {
				Console.WriteLine (message.Key + " : " + filename);
			} else {
				Console.WriteLine ("NOT: " + filename);
			}

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

