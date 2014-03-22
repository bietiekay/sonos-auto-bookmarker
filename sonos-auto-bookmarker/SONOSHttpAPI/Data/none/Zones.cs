using System;
using System.Collections.Generic;

namespace SONOSHttpAPI
{	
	public class SONOSZones
	{
		public string uuid { get; set; }
		public Coordinator coordinator { get; set; }
		public List<Member> members { get; set; }
	}
}
