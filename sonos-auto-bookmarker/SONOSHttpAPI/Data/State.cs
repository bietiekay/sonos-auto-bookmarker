using System;

namespace SONOSHttpAPI
{
	public class State
	{
		public CurrentTrack currentTrack { get; set; }
		public NextTrack nextTrack { get; set; }
		public int volume { get; set; }
		public bool mute { get; set; }
		public int trackNo { get; set; }
		public int elapsedTime { get; set; }
		public string elapsedTimeFormatted { get; set; }
		public string zoneState { get; set; }
		public string playerState { get; set; }
	}
}
