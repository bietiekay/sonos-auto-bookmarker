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

		public SONOSListener (Configuration incomingConfiguration)
		{
			myConfiguration = incomingConfiguration;
			shutdown = false;
			ZonesUpdater = new SONOSZonesUpdater ();
		}

		#region Helper Methods
		public List<State> PlayingNow(List<SONOSZone> Zones)
		{
			List<State> Output = new List<State> ();

			foreach(SONOSZone Zone in Zones)
			{
				if (Zone.coordinator.state.playerState == "PLAYING")
				{
					// we have a winner!
					Output.Add (Zone.coordinator.state);
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

				List<State> NowPlaying = PlayingNow (Zones);


				if (NowPlaying.Count == 0)
					Console.WriteLine ("Nothing playing");

				// go through all zones and find what is being played...
				foreach(State Playing in NowPlaying)
				{
					// we have these information at hand now: title, album, artist, duration
					// and from the State elapsedTime
					// from the track information we build a hash
					String Track = Playing.currentTrack.title + Playing.currentTrack.album + Playing.currentTrack.artist + Convert.ToString(Playing.currentTrack.duration);

					Console.WriteLine (Playing.currentTrack.artist+" - "+Playing.currentTrack.title+" - "+Playing.elapsedTimeFormatted+" ("+GetHashString(Track)+")");
				}
				Thread.Sleep (myConfiguration.GetUpdateIntervalSeconds()*1000);
			}
		}
		#endregion

	}
}

