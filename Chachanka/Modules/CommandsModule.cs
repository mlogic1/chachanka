using Chachanka.Services;
using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Threading.Tasks;

namespace Chachanka.Modules
{
	public class CommandsModule : ModuleBase<SocketCommandContext>
	{
		private readonly AudioService _audioService;

		public CommandsModule(AudioService audioService)
		{
			_audioService = audioService;
		}

		[Command("ping")]
		public async Task Ping()
		{
			await ReplyAsync("pong!");
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
			
			await Context.Client.SetGameAsync("Playing some music for tryharders");
			await _audioService.JoinVoiceChannel(Context.Guild, channel);
			await _audioService.SendAudioAsync(Context.Guild, Context.Channel, "http://194.56.74.7:9302/stream");
		}

		// [Command("vol")] // TODO implement volume control
	}
}
