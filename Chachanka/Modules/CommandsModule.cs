using Chachanka.Services;
using Chachanka.Utility;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Chachanka.Modules
{
	public class CommandsModule : ModuleBase<SocketCommandContext>
	{
		private readonly AudioService _audioService;
		private readonly ConsoleWriterService _consoleWriter;
		private readonly WeatherService _weatherService;

		public CommandsModule(AudioService audioService, ConsoleWriterService consoleWriter, WeatherService weatherService)
		{
			_audioService = audioService;
			_consoleWriter = consoleWriter;
			_weatherService = weatherService;
		}

		[Command("ping")]
		public async Task Ping()
		{
			await ReplyAsync("pong!");
			await _consoleWriter.WriteLogAsync("topkek");
		}

		[Command("join", RunMode = RunMode.Async)]
		public async Task JoinChannel(IVoiceChannel channel = null)
		{
			channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
			if (channel == null)
			{
				await ReplyAsync("Can't join your voice channel");
				return;
			}
			
			await Context.Client.SetGameAsync("some music for tryharders");
			await _audioService.JoinVoiceChannel(Context.Guild, channel);
			await _audioService.SendAudioAsync(Context.Guild, Context.Channel, "http://194.56.74.7:9302/stream");
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
	}
}
