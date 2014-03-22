using System;
using Newtonsoft.Json;
using System.IO;

namespace sonosautobookmarker
{
	public class AutoBookmarkerConfiguration
	{
		public string SONOS_HTTP_API_URL { get; set; }
		public int UpdateIntervalSeconds { get; set; }
	}

	public class Configuration
	{
		private AutoBookmarkerConfiguration myConfiguration;

		public Configuration (String ConfigurationFileName)
		{
			if (File.Exists (ConfigurationFileName)) {
			
				String input = File.ReadAllText (ConfigurationFileName);
				myConfiguration = JsonConvert.DeserializeObject<AutoBookmarkerConfiguration>(input);			
			}
		}

		#region Configuration Access

		public String GetSONOSHTTPAPIURL()
		{
			return myConfiguration.SONOS_HTTP_API_URL;
		}

		public int GetUpdateIntervalSeconds()
		{
			return myConfiguration.UpdateIntervalSeconds;
		}

		#endregion
	}
}

