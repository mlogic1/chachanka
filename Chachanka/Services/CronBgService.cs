using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using chachanka.Interface;
using chachanka.Model.GameDeals;
using Discord;

namespace chachanka.Services
{
	// can be invoked with: echo -n $MESSAGE | nc localhost 9001

	internal class CronBgService
	{
		private readonly ILoggingService _logger;
		private readonly GameDealsService _gameDealsService;

		private readonly DiscordHandleService _discordHandleService;

		private readonly DBService _dbService;

		private readonly TcpListener _tcpListener;

		private CancellationTokenSource _cts;

		public bool IsRunning = true;

		public CronBgService(ILoggingService loggingService, GameDealsService gameDealsService, DiscordHandleService discordHandleService, DBService dbService)
		{
			_logger = loggingService;
			_gameDealsService = gameDealsService;
			_discordHandleService = discordHandleService;
			_dbService = dbService;
			_tcpListener = new TcpListener(IPAddress.Any, 9001);
			_cts = new CancellationTokenSource();
		}

		~CronBgService()
		{
			_tcpListener.Stop();
		}

		public void InitService()
		{
			try
			{
				Task.Run(() => RunInBackground(_cts.Token));
			}
			catch(OperationCanceledException ex)
			{
				_logger.LogInfo($"[CronBgService] Exception occured: {ex.Message}. Trace: {ex.StackTrace}");
			}
		}

		private async Task RunInBackground(CancellationToken ctoken)
		{
			try
			{
				_tcpListener.Start();
				while (!ctoken.IsCancellationRequested)
				{
					await _logger.LogInfo("Waiting for a connection");
					var client = await _tcpListener.AcceptTcpClientAsync();
					_ = Task.Run(() => HandleMessageAsync(client), ctoken);
				}
			}
			catch (Exception ex)
			{
				await _logger.LogInfo(ex.Message);
			}
		}

		private async Task HandleMessageAsync(TcpClient client)
		{
			using (client)
			using (var stream = client.GetStream())
			{
				var buffer = new byte[1024];
				var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
				var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
				await _logger.LogInfo($"Message from network: {message}");

				// check what the message says
				if (message.Equals("tryhard"))
				{
					await NotifyTryhardCommunityWithDeals();
				}
			}
		}

		private async Task NotifyTryhardCommunityWithDeals()
		{
			List<DealSubscriber> subs = await _dbService.GetAllDealSubscribers();

			if (subs.Count == 0)
			{
				return;
			}

			List<Deal> deals = await _gameDealsService.GetCurrentTopDeals();

			await _dbService.StoreDeals(deals);

			EmbedBuilder builder = new EmbedBuilder()
			.WithTitle("Chachanka brings the best rated deals")
			.WithDescription("Here's a list of best rated deals I can find at the moment, which i haven't already posted")
			.WithColor(Color.Blue)
			.WithFooter("Chachanka Deals Finder")
			.WithCurrentTimestamp()
			.WithThumbnailUrl(deals.First().thumb);

			foreach (DealSubscriber sub in subs)
			{
				builder.Fields.Clear();
				foreach (var deal in deals)
				{
					string gameName = deal.title;
					string store = await _gameDealsService.GetStoreName(deal.storeID);
					string price = deal.salePrice;

					int idiscount = (int)float.Parse(deal.savings, CultureInfo.InvariantCulture);
					string discount = idiscount.ToString();

					string releaseDate = "N/A";
					DateTimeOffset releaseOffset = DateTimeOffset.FromUnixTimeSeconds(deal.releaseDate);
					if (releaseOffset.Year > 1970)
					{
						releaseDate = DateTimeOffset.FromUnixTimeSeconds(deal.releaseDate).Date.ToShortDateString();
					}

					string steamRating = "";

					if (deal.steamRatingText != null)
					{
						steamRating += deal.steamRatingText;

						if (deal.steamRatingPercent != null)
						{
							steamRating += $" ({deal.steamRatingPercent}%)";
						}
					}
					else
					{
						steamRating += "N/A";
					}

					if (deal.salePrice == "0.00")
					{
						price = "FREE";
					}
					string description = @$"Price:	${price} ({discount}% off)
Store: {store}
Steam Rating: {steamRating}
Release date: {releaseDate}";
					builder.AddField(gameName, description);
				}

				try
				{
					ulong guildId = ulong.Parse(sub.GuildId);
					ulong channelId = ulong.Parse(sub.ChannelId);

					await _discordHandleService.SendEmbedToChannel(guildId, channelId, builder.Build());
				}
				catch(Exception ex)
				{
					await _logger.LogInfo(ex.Message);
				}
			}
		}
	}
}