using System;
using System.Threading;
using Newtonsoft.Json;

namespace sonosautobookmarker
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("SONOS Auto Bookmarker Tool");
			Console.WriteLine ("(C) Daniel Kirstenpfad 2014 - http://www.technology-ninja.com");
			Console.WriteLine ();

			Configuration myConfiguration = new Configuration ("configuration.json");

			#region Start-Up Main-Event Loop
			Console.WriteLine("Starting SONOS Event listener...");
			SONOSListener _Thread = new SONOSListener(myConfiguration);
			Thread SONOSListenerThread = new Thread(new ThreadStart(_Thread.Run));
			SONOSListenerThread.Start();
			#endregion


		}
	}
}
