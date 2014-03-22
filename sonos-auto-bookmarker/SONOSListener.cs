using System;
using System.Collections.Generic;
using System.Threading;
using SONOSHttpAPI;
using System.Security.Cryptography;
using System.Text;

namespace sonosautobookmarker
{
	public class SONOSListener
	{
		private Configuration myConfiguration;
		private bool shutdown;
		private SONOSZonesUpdater ZonesUpdater;
		private Dictionary<String,int> RememberedPlayPositions;

		public SONOSListener (Configuration incomingConfiguration)
		{
			myConfiguration = incomingConfiguration;
			shutdown = false;
			ZonesUpdater = new SONOSZonesUpdater ();
		}

		#region Helper Methods
		public Dictionary<String,State> PlayingNow(List<SONOSZone> Zones)
		{
			Dictionary<String,State> Output = new Dictionary<String,State> ();

			foreach(SONOSZone Zone in Zones)
			{
				if (Zone.coordinator.state.playerState == "PLAYING")
				{
					// we have a winner!
					Output.Add(Zone.coordinator.roomName,Zone.coordinator.state);
				}
			}

			return Output;
		}

		public static byte[] GetHash(string inputString)
		{
			HashAlgorithm algorithm = MD5.Create();  // SHA1.Create()
			return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
		}

		public static string GetHashString(string inputString)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in GetHash(inputString))
				sb.Append(b.ToString("X2"));

			return sb.ToString();
		}

		#endregion

		#region Thread Method
		public void Run()
		{
			Console.WriteLine ("Trying to connect to : "+myConfiguration.GetSONOSHTTPAPIURL());

			while (!shutdown) {
			
				List<SONOSZone> Zones = ZonesUpdater.UpdateSONOSZones (myConfiguration.GetSONOSHTTPAPIURL ());

				Dictionary<String,State> NowPlaying = PlayingNow (Zones);


				if (NowPlaying.Count == 0)
					Console.WriteLine ("Nothing playing");

				// go through all zones and find what is being played...
				foreach(String coordinator in NowPlaying.Keys)
				{
					State Playing = NowPlaying [coordinator];
					// we have these information at hand now: title, album, artist, duration
					// and from the State elapsedTime
					// from the track information we build a hash
					String Track = Playing.currentTrack.title + Playing.currentTrack.album + Playing.currentTrack.artist + Convert.ToString(Playing.currentTrack.duration);
					String TrackHash = GetHashString (Track);
					double TrackDonePercentage = (float)Playing.elapsedTime / (float)Playing.currentTrack.duration * 100;

					Console.WriteLine (coordinator+" - "+Playing.currentTrack.artist+" - "+Playing.currentTrack.title+" - "+Playing.elapsedTime+" - "+Playing.currentTrack.duration+" - "+String.Format("{0:0}%", TrackDonePercentage)+" ("+TrackHash+")");
				}
				Thread.Sleep (myConfiguration.GetUpdateIntervalSeconds()*1000);
			}
		}
		#endregion

	}
}

