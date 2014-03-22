using System;
using System.Collections.Generic;
using System.Threading;
using SONOSHttpAPI;

namespace sonosautobookmarker
{
	public class SONOSListener
	{
		private Configuration myConfiguration;
		private bool shutdown;
		private SONOSZonesUpdater ZonesUpdater;

		public SONOSListener (Configuration incomingConfiguration)
		{
			myConfiguration = incomingConfiguration;
			shutdown = false;
			ZonesUpdater = new SONOSZonesUpdater ();
		}

		#region Thread Method
		public void Run()
		{
			Console.WriteLine ("Trying to connect to : "+myConfiguration.GetSONOSHTTPAPIURL());

			while (!shutdown) {
			
				List<SONOSZones> Zones = ZonesUpdater.UpdateSONOSZones (myConfiguration.GetSONOSHTTPAPIURL ());

				foreach(SONOSZones zone in Zones)
				{
					Console.WriteLine (zone.uuid);
				}

				Console.Write (".");
				Thread.Sleep (myConfiguration.GetUpdateIntervalSeconds()*1000);
			}
		}
		#endregion

	}
}

