
using System.Net;
using System.Net.Sockets;
using System.Text;
using chachanka.Interface;
using Microsoft.Extensions.Hosting;

namespace chachanka.Services
{
	internal class CronBgService : IHostedService
	{
		private readonly ILoggingService _logger;
		private readonly GameDealsService _gameDealsService;

		public bool IsRunning = true;
		public CronBgService(ILoggingService loggingService, GameDealsService gameDealsService)
		{
			_logger = loggingService;
			_gameDealsService = gameDealsService;
		}

		/*protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				// Define the endpoint and create a socket
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 9000);
				Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				// Bind the socket to the endpoint and start listening
				listener.Bind(endPoint);
				listener.Listen(10);

				await _logger.LogInfo("[CronBGService] Accepting connection...");

				Socket handler = listener.Accept();

				// Buffer for incoming data
				byte[] buffer = new byte[1024];
				int bytesReceived = handler.Receive(buffer);

				// Convert the data to a string and display it
				string data = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
				await _logger.LogInfo($"[CronBGService] Received message: {data}");

				if (data.Equals("MySecretMessage"))
				{
					await _logger.LogInfo("Received a secret message");
				}

				// Close the socket
				handler.Shutdown(SocketShutdown.Both);
				handler.Close();
			}
		}*/

		/*public override async Task StopAsync(CancellationToken cancelToken)
		{
			await base.StopAsync(cancelToken);
		}*/

        public Task StartAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
			{
				// Define the endpoint and create a socket
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 9000);
				Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				// Bind the socket to the endpoint and start listening
				listener.Bind(endPoint);
				listener.Listen(10);

				// await _logger.LogInfo("[CronBGService] Accepting connection...");

				Socket handler = listener.Accept();

				// Buffer for incoming data
				byte[] buffer = new byte[1024];
				int bytesReceived = handler.Receive(buffer);

				// Convert the data to a string and display it
				string data = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
				// await _logger.LogInfo($"[CronBGService] Received message: {data}");

				if (data.Equals("MySecretMessage"))
				{
					// await _logger.LogInfo("Received a secret message");
				}

				// Close the socket
				handler.Shutdown(SocketShutdown.Both);
				handler.Close();
			}
			return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask; // Not sure what to put in here
        }
    }
}