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
			try
			{
				await _client.LoginAsync(TokenType.Bot, _discordBotToken);
				await _client.StartAsync();
			}
			catch(Exception ex)
			{
				await _logger.LogInfo(ex.Message);
				Environment.Exit(-1);
			}
		}
	}
}
