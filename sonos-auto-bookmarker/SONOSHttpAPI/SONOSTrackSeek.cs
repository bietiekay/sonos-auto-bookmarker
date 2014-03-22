using System;
using System.Collections.Generic;
using SONOSHttpAPI;
using System.Net;

namespace sonosautobookmarker
{
	public class SONOSTrackSeek
	{
		public SONOSTrackSeek ()
		{
		}

		public void SeekTrack(String BaseURL,int Position,String Room)
		{
			// create a web client and get the data
			String fullURL = BaseURL+"/"+Room+"/"+Position;

			WebClient client = new WebClient ();

			try
			{
				client.DownloadString(fullURL);
			}
			catch(Exception e)
			{
				Console.WriteLine("SONOS TrackSeek Exception: "+e.Message);
				return;
			}

			return;
		}

	}
}

