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
		private Dictionary<String,State> NowPlayingBefore;
		private Dictionary<String,State> NowPlaying;
		private TransitionManager TManager;

		public SONOSListener (Configuration incomingConfiguration)
		{
			myConfiguration = incomingConfiguration;
			shutdown = false;
			ZonesUpdater = new SONOSZonesUpdater ();
			NowPlaying = null;
			NowPlayingBefore = null;
			TManager = new TransitionManager (incomingConfiguration);
		}

		public void Shutdown()
		{
			shutdown = true;
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
		#endregion

		#region Thread Method
		public void Run()
		{
			Console.WriteLine ("Trying to connect to : "+myConfiguration.GetSONOSHTTPAPIURL());

			while (!shutdown) {
			
				try{
					// get the update from the SONOS system...
					List<SONOSZone> Zones = ZonesUpdater.UpdateSONOSZones (myConfiguration.GetSONOSHTTPAPIURL ());

					// save before state...
					NowPlayingBefore = NowPlaying;
					// get new State...
					NowPlaying = PlayingNow (Zones);

/*					if (NowPlaying.Count == 0)
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
					*/
					// check what changed from the last update...
					if (NowPlayingBefore != null)
					{
						// Check for Transitions
						TManager.CalculateTransitions(NowPlayingBefore,NowPlaying);
					}
				}
				catch(Exception e)
				{
					// pokemon handling, catching them all, because we want this to run "unlimitedly"
					Console.WriteLine ("Pokemon: "+e.Message);
				}
				Thread.Sleep (myConfiguration.GetUpdateIntervalSeconds()*1000);
			}
		}
		#endregion
	}
}

