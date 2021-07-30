using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Chachanka.Utility.RadioLoader;

namespace Chachanka.Services
{
	public class RadioListService
	{
		private List<RadioData> _radioList;

		public RadioListService()
		{
			_radioList = LoadRadioList();
		}

		public Task<RadioData> GetRadioUrl(string radioName)
		{
			RadioData data = _radioList.Find(x => x.ShortName == radioName);

			if (data == null)
			{
				throw new Exception($"Radio {radioName} is not supported");
			}
			else
			{
				return Task.FromResult(data);
			}
		}
	}
}
