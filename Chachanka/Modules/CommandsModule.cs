using Chachanka.Services;
using Chachanka.Utility;
using Discord;
using Discord.Commands;
using System;
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
		private readonly RadioListService _radioListService;

		public CommandsModule(AudioService audioService, ConsoleWriterService consoleWriter, WeatherService weatherService, RadioListService radioListService)
		{
			_audioService = audioService;
			_consoleWriter = consoleWriter;
			_weatherService = weatherService;
			_radioListService = radioListService;
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
			RadioData radioData = null;
			try
			{
				radioData = await _radioListService.GetRadioUrl(radioName);
			}
			catch(Exception ex)
			{
				await ReplyAsync(ex.Message.ToString());
				return;
			}
			
			if (!await JoinChannel())
			{
				return;
			}

			await Context.Client.SetGameAsync($"📻 {radioData.Name}");
			await _audioService.SendAudioAsync(Context.Guild, Context.Channel, radioData.StreamURL);
		}
	}
}
