using Discord;
using Discord.WebSocket;
using SharpLink;
using System;
using System.Threading.Tasks;

namespace Chachanka
{
    partial class Program
    {
        private DiscordSocketClient _client;
        private LavalinkManager _lavalinkManager;
        private const string token = "";

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Console.WriteLine("Chachanka");
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.MessageReceived += OnMessageReceived;
            _client.Ready += InitLavalinkManager;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();


            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task InitLavalinkManager()
        {
            _lavalinkManager = new LavalinkManager(_client, new LavalinkManagerConfig
            {
                RESTHost = "localhost",
                RESTPort = 2333,
                WebSocketHost = "localhost",
                WebSocketPort = 2333,
                Authorization = "",
                TotalShards = 1 
            });
            await _lavalinkManager.StartAsync();
        }

    }
}
