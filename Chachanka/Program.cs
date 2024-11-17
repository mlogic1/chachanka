using Chachanka.Services;
using Chachanka.Utility;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// invite link
// https://discord.com/api/oauth2/authorize?client_id=231087293987946496&permissions=18676770106945&scope=bot

namespace Chachanka
{
	class Program
	{
		static void Main(string[] args)
		{
			new Program().MainAsync().GetAwaiter().GetResult();
		}

		public async Task MainAsync()
		{
			string botToken = "";
			if (File.Exists("token.txt"))
			{
				botToken = File.ReadAllText("token.txt");
			}
			else
			{
				Console.WriteLine("Unable to load bot token");
				return;
			}

			using (var services = ConfigureServices())
			{
				var client = services.GetRequiredService<DiscordSocketClient>();
				
				client.Log += LogAsync;
				client.Connected += async () =>
				{
					await LogAsync(new Discord.LogMessage(LogSeverity.Info, "Main", "Bot connected"));
				};

				// services.GetRequiredService<CommandService>().Log += LogAsync;
				services.GetRequiredService<SlashCommandHandlingService>();
				await client.LoginAsync(TokenType.Bot, botToken);
				await client.StartAsync();
				// await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
				await Task.Delay(Timeout.Infinite);
			}
		}

		private Task LogAsync(LogMessage log)
		{
			Console.WriteLine(log.ToString());
			return Task.CompletedTask;
		}

		private ServiceProvider ConfigureServices()
		{
			var config = new DiscordSocketConfig()
			{
				// GatewayIntents = GatewayIntents.GuildMessages | GatewayIntents.GuildMessageTyping | GatewayIntents.DirectMessages | GatewayIntents.GuildMembers | GatewayIntents.GuildPresences
				MessageCacheSize = 100
			};

			return new ServiceCollection()
				.AddSingleton(config)
				
				.AddSingleton<DiscordSocketClient>()
				// .AddSingleton<CommandService>()
				.AddSingleton<AudioService>()
				// .AddSingleton<CommandHandlingService>()
				.AddSingleton<SlashCommandHandlingService>()
				.AddSingleton<HttpClient>()
				.AddSingleton<ConsoleWriterService>()
				// .AddSingleton<WeatherService>()
				.AddSingleton<RadioListService>()
				.BuildServiceProvider();
		}
	}
}
