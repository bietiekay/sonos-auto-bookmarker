using System;
using System.Collections.Generic;
using SONOSHttpAPI;

namespace sonosautobookmarker
{
	public class TransitionManager
	{
		Configuration myConfiguration;

		public TransitionManager (Configuration incomingConfiguration)
		{
			myConfiguration = incomingConfiguration;
		}

		#region Helper

		#region Merge Zones
		/// <summary>
		/// Merges the coordinator currentTracks to bookmarks and handles conflicts. When title is played in multiple zones it only takes the title played the furthest...
		/// </summary>
		/// <returns>a dictionary where TitleHash = key and State = Value</returns>
		/// <param name="PlayingState">Playing state.</param>
		public Dictionary<String, Bookmark> MergeCoordinatorsAndHandleConflicts(Dictionary<String,State> PlayingState)
		{
			// firs
			Dictionary<String, Bookmark> MergedPlayingTitles = new Dictionary<string, Bookmark> ();

			foreach(String coordinator in PlayingState.Keys)
			{
				// with the coordinator string we can access the different zones
				State Playing = PlayingState [coordinator];
				String TrackHash = HashTrack.GenerateHashTrack (Playing.currentTrack);

				Bookmark thisZoneTrackBookmark = new Bookmark ();

				thisZoneTrackBookmark.Coordinator = coordinator;
				thisZoneTrackBookmark.Album = Playing.currentTrack.album;
				thisZoneTrackBookmark.Artist = Playing.currentTrack.artist;
				thisZoneTrackBookmark.Duration = Playing.currentTrack.duration;
				thisZoneTrackBookmark.Hash = TrackHash;
				thisZoneTrackBookmark.Position = Playing.elapsedTime;
				thisZoneTrackBookmark.Title = Playing.currentTrack.title;

				if (MergedPlayingTitles.ContainsKey(TrackHash))
				{
					// check if it's further played...
					if (MergedPlayingTitles [TrackHash].Position < thisZoneTrackBookmark.Position)
						MergedPlayingTitles.Remove (TrackHash);
				}

				MergedPlayingTitles.Add (TrackHash,thisZoneTrackBookmark);
			}

			return MergedPlayingTitles;
		}
		#endregion

		#region Get Stopped Titles
		/// <summary>
		/// Get's those titles that have been played in the previous update and are not playing in the current one
		/// </summary>
		/// <returns>The titles.</returns>
		/// <param name="OldPlayingTitles">Old playing titles.</param>
		/// <param name="NewPlayingTitles">New playing titles.</param>
		public List<Bookmark> StoppedTitles (Dictionary<String, Bookmark> OldPlayingTitles, Dictionary<String, Bookmark> NewPlayingTitles)
		{
			List<Bookmark> Output = new List<Bookmark> ();

			foreach(String Hash in OldPlayingTitles.Keys)
			{
				if (!NewPlayingTitles.ContainsKey (Hash))
					Output.Add (OldPlayingTitles [Hash]);
			}

			return Output;
		}
		#endregion

		#region Get Started Titles
		public List<Bookmark> StartedTitles (Dictionary<String, Bookmark> OldPlayingTitles, Dictionary<String, Bookmark> NewPlayingTitles)
		{
			List<Bookmark> Output = new List<Bookmark> ();

			foreach(String Hash in NewPlayingTitles.Keys)
			{
				if (!OldPlayingTitles.ContainsKey (Hash))
					Output.Add (NewPlayingTitles [Hash]);
			}

			return Output;
		}
		#endregion

		#endregion


		/// <summary>
		/// Calculates the transitions.
		/// </summary>
		/// <param name="OldPlayingState">Old playing state.</param>
		/// <param name="NewPlayingState">New playing state.</param>
		public void CalculateTransitions(Dictionary<String,State> OldPlayingState, Dictionary<String,State> NewPlayingState)
		{
			// merge all the zones and handle conflicts...
			Dictionary<String, Bookmark> OldPlayingTitles = MergeCoordinatorsAndHandleConflicts (OldPlayingState);
			Dictionary<String, Bookmark> NewPlayingTitles = MergeCoordinatorsAndHandleConflicts (NewPlayingState);

			// now we have all previous and updated play states... let's find which ones stopped and started
			// stopped = in OldPlayingTitles but not in NewPlayingTitles
			// started = in NewPlayingTitles but not in OldPlayingTitles
			List<Bookmark> Stopped = StoppedTitles (OldPlayingTitles, NewPlayingTitles);
			List<Bookmark> Started = StartedTitles (OldPlayingTitles, NewPlayingTitles);

			foreach(Bookmark bookmark in Stopped)
			{
				// output Stopped Information...
				Console.WriteLine (DateTime.Now.ToShortDateString()+" - Stopped: " + bookmark.Artist + " - "+ bookmark.Title+ " - "+bookmark.Position+"/"+bookmark.Duration);

				// check if this track qualifies to be saved...
				if (bookmark.Duration >= myConfiguration.GetBookmarkOnlyLongerThanSeconds())
				{
					// yes it does...
					Console.WriteLine (DateTime.Now.ToShortDateString () + " - Saving Bookmark: " + bookmark.Position + "@" + bookmark.Hash);
					myConfiguration.AddOrUpdateKnownPosition (bookmark);
				}
			}

			foreach(Bookmark bookmark in Started)
			{
				// output Started Information...
				Console.WriteLine (DateTime.Now.ToShortDateString()+" - Started: " + bookmark.Artist + " - "+ bookmark.Title+ " - "+bookmark.Position+"/"+bookmark.Duration);

				Bookmark storedBookmark = myConfiguration.GetBookmarkForHash (bookmark.Hash);

				if (storedBookmark != null)
				{
					// only seek when the current position leads us to believe that the track has been restarted...
					if (bookmark.Position < 10)
					{
						// we've found a play position, seek there!
						Console.WriteLine (DateTime.Now.ToShortDateString () + " - Found Bookmark, now seeking: " + storedBookmark.Position + "@" + bookmark.Hash);
						SONOSTrackSeek.SeekTrack (myConfiguration.GetSONOSHTTPAPIURL(), storedBookmark.Position,bookmark.Coordinator);
					}
				}
			}

			// decide wether we save or not
			myConfiguration.Save ();
		}
	}
}

