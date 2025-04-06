using System;
using Chachanka.Model.Radio;

namespace Chachanka.Services
{
	internal class RadioListService
	{
		private List<RadioStation> _allStations;
		private readonly DBService _dbService;

		public RadioListService(DBService dbService)
		{
			_dbService = dbService;
			_allStations = new List<RadioStation>();
		}


		public async Task<List<RadioStation>> GetRadioStationsAsync()
		{


			return _allStations;
		}
	}
}
