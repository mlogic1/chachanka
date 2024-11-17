using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Chachanka.Utility
{
	public static class Globals
	{
		public static readonly Dictionary<string, RadioStation> RADIO_STATIONS = new Dictionary<string, RadioStation>()
		{
			{ "otvoreni", new RadioStation{ Name="otvoreni", Description = "Otvoreni Radio", StreamURL = "https://stream.otvoreni.hr/otvoreni"} },
			{ "bravo", new RadioStation{ Name="bravo", Description = "Bravo Bjelovar", StreamURL = "http://c5.hostingcentar.com:8059/stream"} }
		};
	}
}
