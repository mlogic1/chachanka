
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
                LavalinkPlayer player = _lavalinkManager.GetPlayer(guildID) ?? await JoinSenderVoiceChannel(guildID, senderID);
                if (player == null)
                {
                    await message.Channel.SendMessageAsync("There are no voice channels on this server");
                }
                else
                {
                    await player.SetVolumeAsync(DefaultAudioVolume);
                    await PlayRadio(player, RadioChannel.Otvoreni);
                }
            }
            else if (message.Content.Equals("!radio101"))
            {
                LavalinkPlayer player = _lavalinkManager.GetPlayer(guildID) ?? await JoinSenderVoiceChannel(guildID, senderID);
                if (player == null)
                {
                    await message.Channel.SendMessageAsync("There are no voice channels on this server");
                }
                else
                {
                    await player.SetVolumeAsync(DefaultAudioVolume);
                    await PlayRadio(player, RadioChannel.Radio101);
                }
            }
            else if (message.Content.Equals("!radio"))
            {
                string radioList = "!otvoreni - Otvoreni radio\n" + 
                    "!radio101 - Radio 101";
                await message.Channel.SendMessageAsync(radioList);
            }
            else if (message.Content.StartsWith("!yt"))
            {
                LavalinkPlayer player = _lavalinkManager.GetPlayer(guildID) ?? await JoinSenderVoiceChannel(guildID, senderID);
                String[] ytArgs = message.Content.Split(' ');

                if (ytArgs.GetLength(0) >= 2)
                {
                    if (player == null)
                    {
                        await message.Channel.SendMessageAsync("There are no voice channels on this server");
                        return;
                    }

                    string youtubeUrl = ytArgs[1];
                    LoadTracksResponse response = await _lavalinkManager.GetTracksAsync(youtubeUrl);
                    //response.Tracks.Append<LavalinkTrack>(new LavalinkTrack());
                    
                    LavalinkTrack track = response.Tracks.First();
                    await player.PlayAsync(track);
                    
                }
                else if (ytArgs.Length == 1)
                {
                    await message.Channel.SendMessageAsync("!yt plays youtube. Example: !yt https://www.youtube.com/watch?v=somevideoonyoutube");
                }
            }
            else if (message.Content.StartsWith("!vol"))
            {
                LavalinkPlayer player = _lavalinkManager.GetPlayer(guildID);
                if (player != null)
                {
                    String[] volumeArgs = message.Content.Split(' ');
                    if (volumeArgs.GetLength(0) >= 2)
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
                LavalinkPlayer player = _lavalinkManager.GetPlayer(guildID);
                await player.StopAsync();
            }
            else
            {

            }
        }

        private async Task<LavalinkPlayer> JoinSenderVoiceChannel(ulong guildID, ulong senderID)
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
                    return await _lavalinkManager.JoinAsync(voiceChannelToJoin);
                }
                return null;
            }
            else
            {
                return await _lavalinkManager.JoinAsync(voiceChannelToJoin);
            }
        }

    }
}