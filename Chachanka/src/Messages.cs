
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
        private async Task OnMessageReceived(SocketMessage message)
        {
            SocketGuildChannel guildChannel =  (SocketGuildChannel)message.Channel;
            ulong guildID = guildChannel.Guild.Id;
            ulong senderID = message.Author.Id;

            if (message.Content.Equals("!test"))
            {
                await message.Channel.SendMessageAsync("Hello");
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
                //SocketVoiceChannel voiceChannel = (SocketVoiceChannel)_client.GetChannel(242030035077300225);
                if (voiceChannelToJoin != null)
                {
                    player = _lavalinkManager.GetPlayer(guildID) ?? await _lavalinkManager.JoinAsync(voiceChannelToJoin);
                    //LoadTracksResponse response = await _lavalinkManager.GetTracksAsync("http://live.radio101.hr:9531/");
                    LoadTracksResponse response = await _lavalinkManager.GetTracksAsync("http://eu10.fastcast4u.com:3180/");
                    
                    
                    //LoadTracksResponse response = await _lavalinkManager.GetTracksAsync($"ytsearch:rock hits");
                    
                    LavalinkTrack track = response.Tracks.First();
                    //LavalinkTrack track = response.Tracks.First();
                    //await player.
                    await player.PlayAsync(track);

                }
                else
                {
                    // todo join first voice channel as a fallback
                    // handle if there's no voice channels
                    await message.Channel.SendMessageAsync("You are not in a voice channel - Join a voice channel first");
                }
            }
            else if (message.Content.Equals("!vol"))
            {
                LavalinkPlayer player = _lavalinkManager.GetPlayer(guildID);
                if (player != null)
                {
                    await player.SetVolumeAsync(5);
                }
            }
            else if (message.Content.Equals("!off"))
            {
                
            }
            else
            {

            }
        }
    }

}