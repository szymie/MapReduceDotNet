using System;


namespace Server
{
    internal class Program
    {
        private static void Main ()
        {
            var appHost = new AppHost ();
            appHost.Init ();
            ushort port = 8888;
            string listeningOn = string.Format ("http://*:{0}/", port);
            appHost.Start (listeningOn);

            Console.WriteLine ("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);
            Console.WriteLine ("Press ENTER to exit...");
			Console.ReadLine ();
        }
    }
}