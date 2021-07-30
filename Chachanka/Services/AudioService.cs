using Chachanka.Utility;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// TODO
// Bot throws exceptions when you move him to another channel

namespace Chachanka.Services
{
	public class AudioService
	{
		private class ServiceVoiceChannel
		{
			public IAudioClient client;
			public double volume;
			private CancellationTokenSource tokenSrc;
			private CancellationToken token;

			public ServiceVoiceChannel(IAudioClient client, double volume)
			{
				this.client = client;
				this.volume = volume;
			}

			private void CancelActiveStream()
			{
				if (tokenSrc != null)
				{
					tokenSrc.Cancel();
				}
			}

			public Task<CancellationToken> SetupCancellationToken()
			{
				CancelActiveStream();
				tokenSrc = new CancellationTokenSource();
				token = tokenSrc.Token;
				return Task.FromResult(token);
			}
		}

		private readonly ConcurrentDictionary<ulong, ServiceVoiceChannel> ConnectedChannels = new ConcurrentDictionary<ulong, ServiceVoiceChannel>();
		private readonly DiscordSocketClient _client;
		private readonly ConsoleWriterService _consoleWriter;
		private const double DEFAULT_VOLUME = 0.02;

		public AudioService(IServiceProvider services)
		{
			_client = services.GetRequiredService<DiscordSocketClient>();
			_consoleWriter = services.GetRequiredService<ConsoleWriterService>();
		}

		public async Task<IAudioClient> JoinVoiceChannel(IGuild guild, IVoiceChannel target)
		{
			ServiceVoiceChannel svc;
			if (ConnectedChannels.TryGetValue(guild.Id, out svc))
			{
				return null;
			}

			if (target.GuildId != guild.Id)
			{
				return null;
			}

			var audioClient = await target.ConnectAsync();
			svc = new ServiceVoiceChannel(audioClient, DEFAULT_VOLUME);

			if (ConnectedChannels.TryAdd(guild.Id, svc))
			{
				await _consoleWriter.WriteLogAsync("Joined audio channel");
			}
			return audioClient;
		}

		public async Task LeaveAudioChannel(IGuild guild)
		{
			ServiceVoiceChannel svc;
			IAudioClient client;
			if (ConnectedChannels.TryRemove(guild.Id, out svc))
			{
				client = svc.client;
				await client.StopAsync();
				await _consoleWriter.WriteLogAsync("Left audio channel");
			}
		}

		public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
		{
			ServiceVoiceChannel svc;
			IAudioClient client;
			if (ConnectedChannels.TryGetValue(guild.Id, out svc))
			{
				client = svc.client;
				CancellationToken cancellationToken = await svc.SetupCancellationToken();
				using (var ffmpeg = CreateStream(path))
				using (var output = ffmpeg.StandardOutput.BaseStream)
				using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
				{
					try
					{
						byte[] data = new byte[1024];
						while (await output.ReadAsync(data, 0, 1024) != 0)
						{
							for (int i = 0; i < 1024 / 2; ++i)
							{
								// convert to 16-bit
								short sample = (short)((data[i * 2 + 1] << 8) | data[i * 2]);

								// scale
								double gain = svc.volume; // value between 0 and 1.0
								sample = (short)(sample * gain + 0.5);

								// back to byte[]
								data[i * 2 + 1] = (byte)(sample >> 8);
								data[i * 2] = (byte)(sample & 0xff);
							}
							Stream strm = new MemoryStream(data);
							try
							{
								await strm.CopyToAsync(discord, cancellationToken);
							}
							catch(Exception ex)
							{
								ffmpeg.Kill();
								await strm.FlushAsync();
							}
						}
					}
					finally
					{
						await discord.FlushAsync();
						await _consoleWriter.WriteLogAsync("Stream ended");
					}
				}
			}
		}

		public Task SetVolumeAsync(IGuild guild, double volume)
		{
			ServiceVoiceChannel svc;
			if (ConnectedChannels.TryGetValue(guild.Id, out svc))
			{
				svc.volume = volume;	
			}

			return Task.CompletedTask;
		}

		private Process CreateStream(string path)
		{
			return Process.Start(new ProcessStartInfo
			{
				FileName = "ffmpeg",
				Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true
			});
		}
	}
}
