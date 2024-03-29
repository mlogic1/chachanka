﻿using Chachanka.Services;
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

				services.GetRequiredService<CommandService>().Log += LogAsync;

				await client.LoginAsync(TokenType.Bot, botToken);
				await client.StartAsync();
				await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
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
			return new ServiceCollection()
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton<CommandService>()
				.AddSingleton<AudioService>()
				.AddSingleton<CommandHandlingService>()
				.AddSingleton<HttpClient>()
				.AddSingleton<ConsoleWriterService>()
				.AddSingleton<WeatherService>()
				.AddSingleton<RadioListService>()
				.BuildServiceProvider();
		}
	}
}
