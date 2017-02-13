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
			Console.WriteLine("new map register ");
			S3ObjectMetadata testData1 = new S3ObjectMetadata ("testBucket", "/tmp/testData1");
			S3ObjectMetadata testData2 = new S3ObjectMetadata ("testBucket", "/tmp/testData2");
			AssemblyMetadata assemblyMetadata = new AssemblyMetadata ("ClientLib", "MyMapper", "MyReduce", new S3ObjectMetadata("testBucket", "/home/gemboj/Polibuda/sem2/piksr/MapReduceDotNet/ClientLib/bin/Debug/ClientLib.dll"));


			Dictionary<string, List<S3ObjectMetadata>> filesToProcess1 = new Dictionary<string, List<S3ObjectMetadata>> ();
			filesToProcess1.Add ("testData1", new List<S3ObjectMetadata>(){testData1});

			Dictionary<string, List<S3ObjectMetadata>> filesToProcess2 = new Dictionary<string, List<S3ObjectMetadata>> ();
			filesToProcess2.Add ("testData2", new List<S3ObjectMetadata>(){testData2});


			Sender.Tell (new RegisterCoordinatorAckMessage(0));
			//Sender.Tell (new NewWorkMessage(0, new WorkConfig(0, "mapUsername", filesToProcess1, assemblyMetadata)));
			//Sender.Tell (new NewWorkMessage(1, new WorkConfig(0, "mapUsername", filesToProcess2, assemblyMetadata)));
		}

		public void Handle (RegisterReduceCoordinatorMessage message){
			Console.WriteLine("new reduce register ");
			S3ObjectMetadata asdf1 = new S3ObjectMetadata ("testBucket", "/tmp/mapUsername-0-0-1-1");
			S3ObjectMetadata qwer1 = new S3ObjectMetadata ("testBucket", "/tmp/mapUsername-0-0-1-2");
			S3ObjectMetadata notaword1 = new S3ObjectMetadata ("testBucket", "/tmp/mapUsername-0-0-1-3");
			S3ObjectMetadata otherword1 = new S3ObjectMetadata ("testBucket", "/tmp/mapUsername-0-0-1-4");
			S3ObjectMetadata asdf2 = new S3ObjectMetadata ("testBucket", "/tmp/mapUsername-0-0-2-1");
			S3ObjectMetadata qwer2 = new S3ObjectMetadata ("testBucket", "/tmp/mapUsername-0-0-2-2");
			AssemblyMetadata assemblyMetadata = new AssemblyMetadata ("ClientLib", "MyWorker", "MyReduce", new S3ObjectMetadata("testBucket", "/home/gemboj/Polibuda/sem2/piksr/MapReduceDotNet/ClientLib/bin/Debug/ClientLib.dll"));


			Dictionary<string, List<S3ObjectMetadata>> filesToProcess1 = new Dictionary<string, List<S3ObjectMetadata>> ();
			filesToProcess1.Add ("asdf", new List<S3ObjectMetadata>(){asdf1, asdf2});
			filesToProcess1.Add ("notAWord", new List<S3ObjectMetadata>(){notaword1, });


			Dictionary<string, List<S3ObjectMetadata>> filesToProcess2 = new Dictionary<string, List<S3ObjectMetadata>> ();
			filesToProcess2.Add ("qwer", new List<S3ObjectMetadata>(){qwer1, qwer2});
			filesToProcess2.Add ("otherWord", new List<S3ObjectMetadata>(){otherword1});


			Sender.Tell (new RegisterCoordinatorAckMessage(1));
			Sender.Tell (new NewWorkMessage (2, new WorkConfig (0, "reduceUsername", filesToProcess1, assemblyMetadata)));
			Sender.Tell (new NewWorkMessage (3, new WorkConfig (0, "reduceUsername", filesToProcess2, assemblyMetadata)));
		}

		public void Handle (NewWorkAckMessage message){
			Console.WriteLine ("workAckMessage: " + message.WorkerId);
		}

		public void Handle (MapWorkFinishedMessage message)
		{
			Console.WriteLine ("Map Work finished: ");
			foreach(KeyValuePair<string, S3ObjectMetadata> pair in message.MapResult){
				Console.WriteLine ("Key: " + pair.Key + " in file: /tmp/" + pair.Value.Filename);
			}
		}

		public void Handle (ReduceWorkFinishedMessage message){
			var filename = "/tmp/" + message.File.Filename;

			Console.WriteLine ("Reduce Work finished with filename " + filename + " and keys: ");

			foreach (string key in message.Keys) {
				Console.Write (key + " ; ");
			}
			Console.WriteLine("");
		}

		public void Handle (WorkerFailureMessage message)
		{
			Console.WriteLine ("worker failure: " + message.Message);
		}

		public void Handle (Terminated message)
		{
			throw new NotImplementedException ();
		}

		public MasterActor ()
		{			

		}
	}
}
