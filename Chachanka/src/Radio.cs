
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
        enum RadioChannel 
        {
            Otvoreni,
            Radio101
        };

        Dictionary<RadioChannel, String> Radios = new Dictionary<RadioChannel, string>
        {
            {   RadioChannel.Otvoreni, "http://eu10.fastcast4u.com:3180/" },
            {   RadioChannel.Radio101, "http://live.radio101.hr:9531/" }
        };

        private async Task PlayRadio(LavalinkPlayer player, RadioChannel channel)
        {
            LoadTracksResponse response = await _lavalinkManager.GetTracksAsync(Radios[channel]);
            LavalinkTrack track = response.Tracks.First();
            await player.PlayAsync(track);
        }
    }

}