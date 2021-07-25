using Chachanka.Services;
using Chachanka.Utility;
using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Chachanka.Utility.RadioLoader;

namespace Chachanka.Modules
{
	public class CommandsModule : ModuleBase<SocketCommandContext>
	{
		private readonly AudioService _audioService;
		private readonly ConsoleWriterService _consoleWriter;
		private readonly WeatherService _weatherService;

		private Dictionary<string, RadioData> _radioList;

		public CommandsModule(AudioService audioService, ConsoleWriterService consoleWriter, WeatherService weatherService)
		{
			_audioService = audioService;
			_consoleWriter = consoleWriter;
			_weatherService = weatherService;
			_radioList = LoadRadioList();
		}

		[Command("ping")]
		public async Task Ping()
		{
			await ReplyAsync("pong!:white_check_mark:");
			await _consoleWriter.WriteLogAsync("topkek");
		}

		[Command("join", RunMode = RunMode.Async)]
		public async Task<bool> JoinChannel(IVoiceChannel channel = null)
		{
			channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
			if (channel == null)
			{
				await ReplyAsync("Can't join your voice channel");
				return false;
			}
			
			await _audioService.JoinVoiceChannel(Context.Guild, channel);
			return true;
		}

		[Command("vol")]
		public async Task AdjustVolume(int num, IVoiceChannel channel = null)
		{
			channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;

			if (channel == null)
			{
				await ReplyAsync("You have to be in a voice channel to adjust volume");
				return;
			}

			if (num < 0 || num > 100)
			{
				await ReplyAsync("Volume must be between 0 and 100");
				return;
			}

			double normalizedVolume = num / 100.0;
			await _audioService.SetVolumeAsync(Context.Guild, normalizedVolume);
		}

		[Command("leave", RunMode = RunMode.Async)]
		public async Task LeaveChannel(IVoiceChannel channel = null)
		{
			await _audioService.LeaveAudioChannel(Context.Guild);
		}

		[Command("weather")]
		[Alias("vrijeme")]
		public async Task ReportWeather()
		{
			if (Context.Channel == null)
			{
				return;
			}
			await _weatherService.ReportWeather(Context.Channel);
		}

		[Command("radio", RunMode = RunMode.Async)]
		public async Task PlayRadio(string radioName)
		{
			if (!await JoinChannel())
			{
				return;
			}



			await _audioService.SendAudioAsync(Context.Guild, Context.Channel, "https://stream.otvoreni.hr/otvoreni");
			await Context.Client.SetGameAsync("some music for tryharders"); // TODO: something with this

			// return Task.CompletedTask;
		}
	}
}
