using System;

namespace SONOSHttpAPI
{
	public class Member
	{
		public string uuid { get; set; }
		public State state { get; set; }
		public PlayMode playMode { get; set; }
		public string roomName { get; set; }
		public string coordinator { get; set; }
		public GroupState groupState { get; set; }
	}
}
