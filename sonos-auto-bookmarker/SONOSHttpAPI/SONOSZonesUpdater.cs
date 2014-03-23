using System;
using System.Collections.Generic;
using SONOSHttpAPI;
using System.Net;
using Newtonsoft.Json;

namespace sonosautobookmarker
{
	/// <summary>
	/// updates all zone information
	/// </summary>
	public class SONOSZonesUpdater
	{
		public SONOSZonesUpdater ()
		{
		}

		public List<SONOSZone> UpdateSONOSZones(String BaseURL)
		{
			// create a web client and get the data
			String fullURL = BaseURL+"/zones";

			WebClient client = new WebClient ();
			client.Encoding = System.Text.Encoding.UTF8;

			List<SONOSZone> Zones = null;

			try
			{
				String JSONInput = client.DownloadString(fullURL);
				//Console.WriteLine("JSON Output: "+JSONInput);
				// hurray, we got a string!
				// let's deserialize it!

				Zones = JsonConvert.DeserializeObject<List<SONOSZone>>(JSONInput);
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

