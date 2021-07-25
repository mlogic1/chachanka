using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Chachanka.Utility
{
	public static class RadioLoader
	{
		public class RadioData
		{
			public string Name { get; set; }
			public string ShortName { get; set; }
			public string StreamURL { get; set; }
		}

		public class RadioJsonRoot
		{
			public Dictionary<string, RadioData> Radios { get; set; }
		}
		

		public static Dictionary<string, RadioData> LoadRadioList()
		{
			RadioJsonRoot radioJsonObj = null;
			if (File.Exists("Radios.json"))
			{
				string data = File.ReadAllText("Radios.json");
				radioJsonObj = JsonSerializer.Deserialize<RadioJsonRoot>(data);
			}

			if (radioJsonObj == null)
			{
				throw new Exception("Unable to load radio list");
			}

			return radioJsonObj.Radios;
		}
	}
}
