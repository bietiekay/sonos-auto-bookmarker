using System;
using System.Collections.Generic;
using SONOSHttpAPI;
using System.Net;
using System.Threading;
using System.Web;

namespace sonosautobookmarker
{
	/// <summary>
	/// seeks in a track
	/// </summary>
	public static class SONOSTrackSeek
	{
		public static void SeekTrack(String BaseURL,int Position,String Room)
		{
			//String encodedRoom = HttpUtility.UrlEncode(Room);

			// create a web client and get the data
			String fullURL = BaseURL+"/"+Room+"/trackseek/"+Position;

			WebClient client = new WebClient ();
			client.Encoding = System.Text.Encoding.UTF8;

			try
			{
				// 1 second wait
				//Thread.Sleep(1000);
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

