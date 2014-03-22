using System;
using System.Collections.Generic;
using SONOSHttpAPI;
using System.Net;
using Newtonsoft.Json;

namespace sonosautobookmarker
{
	public class SONOSZonesUpdater
	{
		public SONOSZonesUpdater ()
		{
		}

		public List<SONOSZones> UpdateSONOSZones(String BaseURL)
		{
			// create a web client and get the data
			String fullURL = BaseURL+"/zones";

			WebClient client = new WebClient ();

			List<SONOSZones> Zones = null;

			try
			{
				String JSONInput = client.DownloadString(fullURL);
				//Console.WriteLine("JSON Output: "+JSONInput);
				// hurray, we got a string!
				// let's deserialize it!

				Zones = JsonConvert.DeserializeObject<List<SONOSZones>>(JSONInput);
			}
			catch(Exception e)
			{
				Console.WriteLine("SONOS Zones Updater Exception: "+e.Message);
				return null;
			}

			return Zones;
		}

	}
}

