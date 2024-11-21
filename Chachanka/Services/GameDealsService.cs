using chachanka.Interface;
using chachanka.Model.GameDeals;
using System.Text.Json;

namespace chachanka.Services
{
	internal class GameDealsService
	{
		private readonly ILoggingService _logger;

		private readonly DBService _dbService;

		private List<Store> Stores = new List<Store>();

		private const string CSHARK_API_STORES_INFO = "https://www.cheapshark.com/api/1.0/stores";

		private const int TARGET_DEALS = 10;

		public GameDealsService(ILoggingService loggingService, DBService dbService)
		{
			_logger = loggingService;
			_dbService = dbService;
		}

		public async Task RefreshStoreList()
		{
			try
			{
				using (HttpClient client = new HttpClient())
				{
					HttpResponseMessage response = await client.GetAsync(CSHARK_API_STORES_INFO);
					response.EnsureSuccessStatusCode();
					string responseBody = await response.Content.ReadAsStringAsync();
					// await _logger.LogInfo(responseBody);

					List<Store>? temp = JsonSerializer.Deserialize<List<Store>>(responseBody);
					if (temp != null)
					{
						Stores.Clear();
						Stores = temp;
						await _logger.LogInfo("[DBService] Refreshed store list");
					}
					else
					{
						throw new Exception("Unable to refresh store list. Keeping old list");
					}
				}
			}
			catch(Exception ex)
			{
				await _logger.LogInfo("[GameDealsService] Error thrown during RefreshStores: " + ex.Message.ToString());
			}
		}

		public Task<string> GetStoreName(string storeId)
		{
			Store? found = Stores.FirstOrDefault(c => c.storeID.Equals(storeId), null);

			if (found == null)
			{
				return Task.FromResult("Unknown store");
			}
			else
			{
				return Task.FromResult(found.storeName);
			}
		}

		public async Task<List<Deal>> GetCurrentTopDeals()
		{
			List<Deal> topDeals = new List<Deal>();
			try
			{
				using (HttpClient client = new HttpClient())
				{
					int pageNumber = 0;
					int totalPages = -1;

					HttpResponseMessage response = await client.GetAsync($"https://www.cheapshark.com/api/1.0/deals?upperPrice=15&pageNumber={pageNumber}");
					response.EnsureSuccessStatusCode();
					string responseBody = await response.Content.ReadAsStringAsync();

					if (response.Headers.TryGetValues("x-total-page-count", out var atotalPages))
					{
						string totalPageCount = string.Join(",", atotalPages);
						if (int.TryParse(totalPageCount, out totalPages))
						{
							await _logger.LogInfo($"There are {totalPages} pages to go through.");
						}
					}
					else
					{
						await _logger.LogInfo("Header 'X-Total-Page-Count' not found.");
					}

					if (totalPages != -1)
					{
						while(pageNumber <= totalPages)
						{
							await Task.Delay(2500);
							// Query Get
							HttpResponseMessage page_response = await client.GetAsync($"https://www.cheapshark.com/api/1.0/deals?upperPrice=15&pageNumber={pageNumber}");
							page_response.EnsureSuccessStatusCode();

							string page_content = await page_response.Content.ReadAsStringAsync();
							// Parse all deals into a list
							List<Deal>? deals = JsonSerializer.Deserialize<List<Deal>>(page_content);

							await _logger.LogInfo($"[GameDealsService] Processing page {pageNumber}.");

							if (deals == null)
							{
								await _logger.LogInfo("[GameDealsService] Unable to parse response content into json");
								return topDeals;
							}

							// Go through the list, if a deal has not yet been seen, add it to the list
							foreach	(Deal deal in deals)
							{
								// if the deal was seen, skip it
								// otherwise the deal gets added to the good deals (it gets added by CronBgService)
								if (!await _dbService.DealExistsInDb(deal))
								{
									topDeals.Add(deal);
								}

								// once the list has 10 or 20 (or whatever target is) values, BREAK or return, this function should now stop and continue tomorrow or another time
								if (topDeals.Count >= TARGET_DEALS)
								{
									await _logger.LogInfo($"Found top {topDeals.Count} deals");
									return topDeals;
								}
							}

							// at the end, run a delay of about 5-10 seconds, maybe more, to avoid hitting rate limits. This function can end up running 50 times
							++pageNumber;
							await Task.Delay(10000);
						}
					}
				}
			}
			catch (Exception ex)
			{
				await _logger.LogInfo("[GameDealsService] Error thrown during GetCurrentTopDeals: " + ex.Message.ToString());
				if (ex.StackTrace != null)
				{
					await _logger.LogInfo(ex.StackTrace.ToString());
				}
				return new List<Deal>();
			}

			return topDeals;
		}
	}
}
