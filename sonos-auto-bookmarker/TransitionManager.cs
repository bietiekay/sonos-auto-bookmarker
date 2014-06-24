using System;
using System.Text.RegularExpressions;
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
			// first
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

		#region Transition calculations
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
				Console.WriteLine (DateTime.Now.ToShortTimeString()+" - "+DateTime.Now.ToShortDateString()+" - Stopped: " + bookmark.Artist + " - "+ bookmark.Title+ " - "+bookmark.Position+"/"+bookmark.Duration);

				// check if this track qualifies to be saved...
				// these are the requirements:
				//  - the track that stopped is longer than the configured number of seconds (BookmarkOnlyLongerThanSeconds in configuration.json)
				//  - the position within the track is not within the last seconds (number of seconds configurable in configuration.json by UpdateIntervalSeconds)
				if (bookmark.Duration >= myConfiguration.GetBookmarkOnlyLongerThanSeconds())
				{
					bool matchesTitlePattern = false;
					// further check if this one matches any pattern

					#region Regular Expression Check for the title of the track
					foreach(String RegExpPattern in myConfiguration.GetIgnoreTitleNamePatterns())
					{
						Match match = Regex.Match(bookmark.Title, RegExpPattern, RegexOptions.IgnoreCase);

						// Here we check the Match instance.
						if (match.Success)
						{
							Console.WriteLine(DateTime.Now.ToShortTimeString()+" - "+DateTime.Now.ToShortDateString () + " - not saving since title matches ignore pattern: "+RegExpPattern);
							matchesTitlePattern = true;
							break;
						}
					}
					#endregion

					if (!matchesTitlePattern) {
						// check if this bookmark is within the last UpdateIntervalSeconds of the track - then we do not save but we delete the bookmark
						if ((bookmark.Position != 0) && (bookmark.Position <= (bookmark.Duration - myConfiguration.GetUpdateIntervalSeconds ()))) {
							// yes it does...so save it!
							Console.WriteLine (DateTime.Now.ToShortTimeString()+" - "+DateTime.Now.ToShortDateString () + " - Saving Bookmark: " + bookmark.Position + "@" + bookmark.Hash);
							myConfiguration.AddOrUpdateKnownPosition (bookmark);
							myConfiguration.Save ();
						} else {
							// no it does not...so delete it!
							myConfiguration.GetBookmarkForHash (bookmark.Hash);
						}
					}
				}
			}

			foreach(Bookmark bookmark in Started)
			{
				// output Started Information...
				Console.WriteLine (DateTime.Now.ToShortTimeString()+" - "+DateTime.Now.ToShortDateString()+" - Started: " + bookmark.Artist + " - "+ bookmark.Title+ " - "+bookmark.Position+"/"+bookmark.Duration);
				
                if (bookmark.Position < 10)
                {
                    Bookmark storedBookmark = myConfiguration.GetBookmarkForHash(bookmark.Hash);
				    if (storedBookmark != null)
				    {
					    // only seek when the current position leads us to believe that the track has been restarted...
					
						    // we've found a play position, seek there!
							Console.WriteLine (DateTime.Now.ToShortTimeString()+" - "+DateTime.Now.ToShortDateString () + " - Found Bookmark, now seeking: " + storedBookmark.Position + "@" + bookmark.Hash);
						    SONOSTrackSeek.SeekTrack (myConfiguration.GetSONOSHTTPAPIURL(), storedBookmark.Position,bookmark.Coordinator);
                            myConfiguration.Save();
				    }
				}
			}

			// decide wether we save or not		
		}
		#endregion
	}
}

