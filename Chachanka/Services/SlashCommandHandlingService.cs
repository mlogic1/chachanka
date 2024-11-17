using Chachanka.Utility;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Chachanka.Services
{
	public class SlashCommandHandlingService
	{
		private readonly CommandService _commands;
		private readonly DiscordSocketClient _discord;
		private readonly IServiceProvider _services;
		private readonly AudioService _audioService;

		public SlashCommandHandlingService(IServiceProvider services)
		{
			_services = services;
			_discord = services.GetRequiredService<DiscordSocketClient>();
			_discord.SlashCommandExecuted += OnSlashCommandExecuted;

			_discord.Ready += SetupGlobalSlashCommands;

			_audioService = services.GetRequiredService<AudioService>();
		}

		private async Task SetupGlobalSlashCommands()
		{
			SlashCommandBuilder globalCommandBuilder = new SlashCommandBuilder();
			SlashCommandOptionBuilder opbuilder = new SlashCommandOptionBuilder();

			opbuilder.WithName("station").WithDescription("Choose your station").WithType(ApplicationCommandOptionType.Integer);

			/* Radio stations */
			int i = 0;
			foreach (var station in Globals.RADIO_STATIONS)
			{				
					opbuilder.AddChoice(station.Value.Name, i);
				++i;
			}

			/* volume control */
			SlashCommandOptionBuilder volOpbuilder = new SlashCommandOptionBuilder();
			volOpbuilder.WithName("volume").WithDescription("Change volume").WithType(ApplicationCommandOptionType.Number).WithMinValue(0).WithMaxValue(100);

			/* weather forecast */
			SlashCommandOptionBuilder weatherOpBuilder = new SlashCommandOptionBuilder();
			weatherOpBuilder.WithName("weather").WithDescription("Get weather forecast").WithType(ApplicationCommandOptionType.String);

			Console.WriteLine("Setting up slash commands");
			globalCommandBuilder
				.WithName("radio")
				.WithDescription("Play live radio in your channel.")
				.AddOption(opbuilder)
				.AddOption(volOpbuilder);
				// .AddOption(weatherOpBuilder) // move this to a seperate slash command

			try
			{
				await _discord.CreateGlobalApplicationCommandAsync(globalCommandBuilder.Build());
			}
			catch (HttpException exception)
			{
				// If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
				var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

				// You can send this error somewhere or just print it to the console, for this example we're just going to print it.
				Console.WriteLine("Error in slash commands");
				Console.WriteLine(json);
			}
		}

		private async Task OnSlashCommandExecuted(SocketSlashCommand command)
		{
			switch (command.CommandName)
			{
				case "radio":
					await ProcessRadioCommand(command);
					break;

				case "weather":
					await ProcessWeatherCommand(command);
					break;

				default:
					break;

			}
			await Task.CompletedTask;
		}

		private async Task ProcessRadioCommand(SocketSlashCommand command)
		{
			string subcommand = command.Data.Options.ElementAt(0).Name;

			if (subcommand == "station")
			{
				if (command.Data.Options.Count == 0)
				{
					await command.RespondAsync("I can play a radio station");
				}
				else
				{
					Console.WriteLine("Trying to play/change radio station");
					IVoiceChannel vc = (command.User as IGuildUser)?.VoiceChannel;
					IGuild guild = (command.User as IGuildUser).Guild;
					ulong guildId = (command.User as IGuildUser).GuildId;
					if (vc != null)
					{
						Int64 index = (Int64)command.Data.Options.First().Value;
						string streamURL = Globals.RADIO_STATIONS.ElementAt((int)index).Value.StreamURL;
						await command.RespondAsync("Playing radio");

						try
						{
							await _audioService.PlayRadioStream(guildId, vc, streamURL);
						}
						catch (Exception exception)
						{
							Console.WriteLine(exception.ToString());
						}
					}
					else
					{
						await command.RespondAsync("You don't appear to be in a voice channel");
					}
					// check the choice and do something based on it
				}
			}
			else if (subcommand == "volume")
			{
				// ulong guildId = (command.User as IGuildUser).GuildId;
				IGuild guild = (command.User as IGuildUser).Guild;
				await _audioService.SetVolumeAsync(guild, 0.6);
				await command.RespondAsync("Changing volume");
			}
			
		}

		private async Task ProcessWeatherCommand(SocketSlashCommand command)
		{
			await Task.CompletedTask;
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

			if (command.Value.Name.Equals("radio"))
			{
				await context.Channel.SendMessageAsync("I can play different radios. Select which one by writing !radio followed by radio name. `!radio otvoreni`. Todo complete list of radios");
				return;
			}

			// the command failed, let's notify the user that something happened.
			await context.Channel.SendMessageAsync($"error: {result}");
		}
	}
}
