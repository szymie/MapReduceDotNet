using System;
using Akka.Actor;
using Akka.Configuration;
using MapReduceDotNetLib;
using System.Collections.Generic;

namespace Master
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			ActorSystem system = getActorSystem ();

			var master = system.ActorOf<MasterActor>("MasterActor");


			var InputFiles = new Dictionary<string, S3ObjectMetadata>();

			InputFiles.Add("a.txt", new S3ObjectMetadata("", "/tmp/a.txt"));
			InputFiles.Add("b.txt", new S3ObjectMetadata("", "/tmp/b.txt"));
			InputFiles.Add("c.txt", new S3ObjectMetadata("", "/tmp/b.txt"));
			InputFiles.Add("d.txt", new S3ObjectMetadata("", "/tmp/b.txt"));

			var newTask = new NewTaskMessage()
			{
				M = 6,
				InputFiles = InputFiles,
				TaskId = 1,
				Username = "szymie"
			};

			master.Tell(newTask);

			Console.ReadLine();
		}

		private static ActorSystem getActorSystem(){
			AkkaConfig akkaConfig = getAkkaConfig ();

			var configString = String.Format(@"
				akka {{  
					actor {{
						provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
					}}
					remote {{
						helios.tcp {{
							port = {0}
							hostname = {1}
							public-hostname = {2}
						}}
					}}
				}}				
			", akkaConfig.Port, akkaConfig.Hostname, akkaConfig.PublicHostname);

			var config = ConfigurationFactory.ParseString (configString);

			return ActorSystem.Create("MasterSystem", config);
		}

		private static AkkaConfig getAkkaConfig(){
			string port = Environment.GetEnvironmentVariable ("MASTER_PORT");
			if (port == null) {
				port = "8081";
				Console.WriteLine ("No MASTER_PORT found.");
			}

			string hostname = Environment.GetEnvironmentVariable ("MASTER_HOSTNAME");
			if (hostname == null) {
				hostname = "0.0.0.0";
				Console.WriteLine ("No MASTER_HOSTNAME found.");
			}

			string publicHostname = Environment.GetEnvironmentVariable ("MASTER_PUBLICHOSTNAME");
			if (publicHostname == null) {
				publicHostname = "localhost";
				Console.WriteLine ("No MASTER_PUBLICHOSTNAME found.");
			}

			Console.WriteLine ("port: " + port);
			Console.WriteLine ("hostname: " + hostname);
			Console.WriteLine ("public hostname: " + publicHostname);

			return new AkkaConfig (port, hostname, publicHostname);
		}
	}

	class AkkaConfig{
		public AkkaConfig(string port, string hostname, string publicHostname){
			Port = port;
			Hostname = hostname;
			PublicHostname = publicHostname;
		}

		public string Port { get; private set; }
		public string Hostname { get; private set; }
		public string PublicHostname { get; private set; }
	}
}
