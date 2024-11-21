using chachanka.Interface;
using chachanka.Model.GameDeals;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace chachanka.Services
{
	internal class DiscordHandleService
	{
		private DiscordSocketClient _client;
		private readonly ILoggingService _logger;
		private readonly GameDealsService _gameDealService; // remove this param

		private readonly IConfiguration _configuration;

		private readonly string _discordBotToken;

		public DiscordHandleService(ILoggingService loggingService, GameDealsService gameDealService, IConfiguration configuration)
		{
			_configuration = configuration;
			string? botToken = _configuration["BOTToken"];
			if (botToken == null)
			{
				throw new Exception("Missing environment variable BOTToken");
			}
			else
			{
				_discordBotToken = botToken;
			}
			_logger = loggingService;
			_gameDealService = gameDealService;
			_client = new DiscordSocketClient();
			_client.Log += LogConsole;
		}

		public async Task<bool> SendMessageToChannel(ulong guildId, ulong channelId, string message)
		{
			try
			{
				await _client.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync(text: message);
			}
			catch (Exception ex)
			{
				await _logger.LogInfo(ex.Message);
				return false;
			}

			return true;
		}

		public async Task<bool> SendEmbedToChannel(ulong guildId, ulong channelId, Embed message)
		{
			try
			{
				await _client.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync(embed: message);
			}
			catch (Exception ex)
			{
				await _logger.LogInfo(ex.Message);
				return false;
			}

			return true;
		}

		private async Task LogConsole(LogMessage message)
		{
			await _logger.LogInfo(message.ToString());
		}

		public async Task StartService()
		{
			await _client.LoginAsync(TokenType.Bot, _discordBotToken);
			await _client.StartAsync();

			_client.Ready += async () =>
			{
				return;
				List<Deal> deals = await _gameDealService.GetCurrentTopDeals();

				EmbedBuilder builder = new EmbedBuilder()
				.WithTitle("Best rated deals")
				.WithDescription("Here's a list of best rated deals I can find at the moment, which i haven't already posted")
				.WithColor(Color.Blue)
				.WithFooter("Chachanka Deals Finder")
				.WithCurrentTimestamp()
				.WithThumbnailUrl(deals.First().thumb);

				foreach (var deal in deals)
				{
					string gameName = deal.title;
					string store = await _gameDealService.GetStoreName(deal.storeID);
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
					builder.AddField(deal.title, description);
				}

				await SendEmbedToChannel(242029979909619713, 242029979909619713, builder.Build());
			};
		}
	}
}
