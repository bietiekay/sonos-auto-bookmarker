using System;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace sonosautobookmarker
{
	public class AutoBookmarkerConfiguration
	{
		public string SONOS_HTTP_API_URL { get; set; }
		public int UpdateIntervalSeconds { get; set; }
		public int BookmarkOnlyLongerThanSeconds { get; set; }
		public int MinimalSecondsPerSave { get; set; }
		public int MinimalChangesPerSave { get; set; }
		public List<Bookmark> Bookmarks { get; set; }
	}

	public class Configuration
	{
		private AutoBookmarkerConfiguration myConfiguration;
		private DateTime LastSave;
		private int ChangesSinceLastSave;
		private String myConfigurationFile;

		public Configuration (String ConfigurationFileName)
		{
			myConfigurationFile = ConfigurationFileName;

			if (File.Exists (ConfigurationFileName)) {
			
				String input = File.ReadAllText (ConfigurationFileName);
				myConfiguration = JsonConvert.DeserializeObject<AutoBookmarkerConfiguration>(input);			
			}
			if (myConfiguration.Bookmarks == null)
				myConfiguration.Bookmarks = new List<Bookmark> ();
		}

		#region Save State
		/// <summary>
		/// Save the specified ConfigurationFilename when the previosuly configured criterias are met.
		/// </summary>
		public void Save()
		{
            if ((SecondsSinceLastSave() >= myConfiguration.MinimalSecondsPerSave) || (NumberOfChangesSinceLastSave() >= myConfiguration.MinimalChangesPerSave))
			{
				Console.WriteLine (DateTime.Now.ToShortDateString () + " - Saving Configuration.");
				string output = JsonConvert.SerializeObject(myConfiguration);
				LastSave = DateTime.Now;
				ChangesSinceLastSave = 0;
				File.WriteAllText (myConfigurationFile, output);
			}
		}

		public int SecondsSinceLastSave()
		{
			TimeSpan span = new TimeSpan (DateTime.Now.Ticks - LastSave.Ticks);

			return (int)span.TotalSeconds;
		}

		public int NumberOfChangesSinceLastSave()
		{
			return ChangesSinceLastSave;
		}

		#endregion

		#region Bookmark Methods
		/// <summary>
		/// Adds the known position and overwrites if already in...
		/// </summary>
		/// <param name="newBookmark">New bookmark.</param>
		public void AddOrUpdateKnownPosition(Bookmark newBookmark)
		{
			bool found = false;
			int position = 0;

			// check if already exists...
			foreach(Bookmark _bookmark in myConfiguration.Bookmarks)
			{

				if (newBookmark.Hash == _bookmark.Hash)
				{
					found = true;
					break;
				}
                else
				    position++;
			}

			if (found)
			{
				myConfiguration.Bookmarks [position] = newBookmark;
			}
            else
                myConfiguration.Bookmarks.Add(newBookmark);

			ChangesSinceLastSave++;
			
		}

		/// <summary>
		/// Gets the bookmark for hash and deletes bookmark immediately. Returns null when not found. 
		/// </summary>
		/// <returns>The bookmark for hash.</returns>
		/// <param name="Hash">Hash.</param>
		public Bookmark GetBookmarkForHash(String Hash)
		{
			int position = 0;
			bool found = false;

			foreach(Bookmark _bookmark in myConfiguration.Bookmarks)
			{
				if (Hash == _bookmark.Hash) {
					found = true;
                    break;
				} else
					position++;
			}

			// when we got the position we return and remove the bookmark
			if (found)
			{
				Bookmark Output = myConfiguration.Bookmarks [position];
				myConfiguration.Bookmarks.RemoveAt (position);
				ChangesSinceLastSave++;
				return Output;
			}

			return null;
		}
		#endregion

		#region Configuration Access

		public String GetSONOSHTTPAPIURL()
		{
			return myConfiguration.SONOS_HTTP_API_URL;
		}

		public int GetUpdateIntervalSeconds()
		{
			return myConfiguration.UpdateIntervalSeconds;
		}

		public int GetBookmarkOnlyLongerThanSeconds()
		{
			return myConfiguration.BookmarkOnlyLongerThanSeconds;
		}

		public int GetMinimalChangesPerSave()
		{
			return myConfiguration.MinimalChangesPerSave;
		}

		public int GetMinimalSecondsPerSave()
		{
			return myConfiguration.MinimalSecondsPerSave;
		}

		#endregion
	}
}

