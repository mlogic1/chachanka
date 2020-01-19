
using Discord;
using Discord.WebSocket;
using SharpLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chachanka
{
    partial class Program
    {
        private uint DefaultAudioVolume = 2;

        private async Task OnMessageReceived(SocketMessage message)
        {
            SocketGuildChannel guildChannel =  (SocketGuildChannel)message.Channel;
            ulong guildID = guildChannel.Guild.Id;
            ulong senderID = message.Author.Id;

            if (message.Content.Equals("!test"))
            {
                await message.Channel.SendMessageAsync("Hello");
            }
            else if (message.Content.Equals("!otvoreni"))
            {
                if (await JoinSenderVoiceChannel(guildID, senderID))
                {
                    LavalinkPlayer player = _lavalinkManager.GetPlayer(guildID);
                    await PlayRadio(player, RadioChannel.Otvoreni);
                }
                else
                {
                    // TODO test this
                    await message.Channel.SendMessageAsync("There are no voice channels on this server");
                }
            }
            else if (message.Content.Equals("!radio101"))
            {

            }
            else if (message.Content.Equals("!radio"))
            {
                // TODO
            }
            else if (message.Content.Equals("!yt"))
            {
                IReadOnlyCollection<SocketVoiceChannel> voiceChannels = _client.GetGuild(guildID).VoiceChannels;
                SocketVoiceChannel voiceChannelToJoin = null;

                foreach(SocketVoiceChannel channel in voiceChannels)
                {
                    foreach(var user in channel.Users)
                    {
                        if (user.Id == senderID)
                        {
                            voiceChannelToJoin = channel;
                        }
                    }
                }

                LavalinkPlayer player = null;
                if (voiceChannelToJoin != null)
                {
                    player = _lavalinkManager.GetPlayer(guildID) ?? await _lavalinkManager.JoinAsync(voiceChannelToJoin);
                    LoadTracksResponse response = await _lavalinkManager.GetTracksAsync("http://eu10.fastcast4u.com:3180/");
                    
                    LavalinkTrack track = response.Tracks.First();
                    await player.PlayAsync(track);
                }
                else
                {
                    // todo join first voice channel as a fallback
                    // handle if there's no voice channels
                    await message.Channel.SendMessageAsync("You are not in a voice channel - Join a voice channel first");
                }
            }
            else if (message.Content.StartsWith("!vol"))
            {
                LavalinkPlayer player = _lavalinkManager.GetPlayer(guildID);
                if (player != null)
                {
                    String[] volumeArgs = message.Content.Split(' ');
                    if (volumeArgs.Length >= 2)
                    {
                        uint newVolume = 0;
                        if (uint.TryParse(volumeArgs[1], out newVolume))
                        {
                            if (newVolume >= 0 && newVolume <= 100)
                            {
                                await player.SetVolumeAsync(newVolume);
                            }
                            else
                            {
                                await message.Channel.SendMessageAsync("Volume must be between 0 and 100");
                            }
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("Value after !vol must be a number between 0 and 100");
                        }
                    }
                    else if (volumeArgs.Length == 1)
                    {
                        await message.Channel.SendMessageAsync("!vol changes audio volume. Example: !vol 10");
                    }
                }
            }
            else if (message.Content.Equals("!off"))
            {
                
            }
            else
            {

            }
        }

        private async Task<bool> JoinSenderVoiceChannel(ulong guildID, ulong senderID)
        {
            IReadOnlyCollection<SocketVoiceChannel> voiceChannels = _client.GetGuild(guildID).VoiceChannels;
            SocketVoiceChannel voiceChannelToJoin = null;

            foreach(SocketVoiceChannel channel in voiceChannels)
            {
                foreach(var user in channel.Users)
                {
                    if (user.Id == senderID)
                    {
                        voiceChannelToJoin = channel;
                    }
                }
            }

            if (voiceChannelToJoin == null)
            {
                if (voiceChannels.Count > 0)
                {
                    voiceChannelToJoin = voiceChannels.First();
                    await _lavalinkManager.JoinAsync(voiceChannelToJoin);
                    return true;
                }
                return false;
            }

            return true;
        }

    }
}