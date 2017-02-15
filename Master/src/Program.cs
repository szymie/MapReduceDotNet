using System;
using Akka.Actor;
using Akka.Configuration;
using MapReduceDotNetLib;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;

namespace Master
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			ActorSystem system = getActorSystem ();

			var master = system.ActorOf<MasterActor>("MasterActor");





			ManualResetEvent resetEvent = new ManualResetEvent(false);
			resetEvent.WaitOne();
		}

		private static ActorSystem getActorSystem(){
			AkkaConfig akkaConfig = getAkkaConfig ();
			//stdout-loglevel = OFF
			//loglevel = OFF
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
				publicHostname = getIpv4Eth0 ();

				if(publicHostname == null){
					publicHostname = "localhost";
				}
				Console.WriteLine ("No MASTER_PUBLICHOSTNAME found.");
			}

			Console.WriteLine ("port: " + port);
			Console.WriteLine ("hostname: " + hostname);
			Console.WriteLine ("public hostname: " + publicHostname);

			return new AkkaConfig (port, hostname, publicHostname);
		}

		private static string getIpv4Eth0(){
			try{
				foreach(NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
				{
					if(ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
					{
						//Console.WriteLine(ni.Name);
						if(ni.Name.Equals("eth0")){
							foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
							{
								if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
								{
									return ip.Address.ToString();
								}
							}
						}
					}  
				}		
			}
			catch(Exception e){
				return null;
			}

			return null;
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
