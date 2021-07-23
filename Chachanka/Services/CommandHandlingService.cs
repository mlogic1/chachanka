using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Chachanka.Services
{
	public class CommandHandlingService
	{
		private readonly CommandService _commands;
		private readonly DiscordSocketClient _discord;
		private readonly IServiceProvider _services;

		public CommandHandlingService(IServiceProvider services)
		{
			_commands = services.GetRequiredService<CommandService>();
			_discord = services.GetRequiredService<DiscordSocketClient>();
			_services = services;

			_commands.CommandExecuted += CommandExecutedAsync;
			_discord.MessageReceived += MessageReceivedAsync;
		}

		public async Task InitializeAsync()
		{
			// Register modules that are public and inherit ModuleBase<T>.
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
		}

		public async Task MessageReceivedAsync(SocketMessage rawMessage)
		{
			// Ignore system messages, or messages from other bots
			if (!(rawMessage is SocketUserMessage message)) return;
			if (message.Source != MessageSource.User) return;

			
			var argPos = 0;
			if (!message.HasCharPrefix('!', ref argPos))
			{
				return;
			}
			//if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) return;

			var context = new SocketCommandContext(_discord, message);
			await _commands.ExecuteAsync(context, argPos, _services);
		}

		public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
		{
			// command is unspecified when there was a search failure (command not found); we don't care about these errors
			if (!command.IsSpecified)
				return;

			// the command was successful, we don't care about this result, unless we want to log that a command succeeded.
			if (result.IsSuccess)
				return;

			if (command.Value.Name.Equals("vol"))
			{
				await context.Channel.SendMessageAsync("You can change the volume by writing !vol followed by volume amount. `!vol 40`. The value must be between 0 and 100");
				return;
			}

			// the command failed, let's notify the user that something happened.
			await context.Channel.SendMessageAsync($"error: {result}");
		}
	}
}
