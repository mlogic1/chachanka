using chachanka.Interface;
using Discord;
using System.Collections.Concurrent;

namespace chachanka.Services
{
	internal class ConsoleLoggerService : ILoggingService
	{
		private BlockingCollection<string> _messageQueue;

		public ConsoleLoggerService()
		{
			_messageQueue = new BlockingCollection<string>();

			var thread = new Thread(WriteFromQueue)
			{
				IsBackground = true
			};

			thread.Start();
		}

		private void WriteFromQueue()
		{
			// This must be run on a thread
			while (true)
			{
				Console.WriteLine(_messageQueue.Take());
			}
		}

		public void WriteLog(string text)
		{
			_messageQueue.Add(text);
		}

		public Task WriteLogAsync(string text)
		{
			_messageQueue.Add(text);

			return Task.CompletedTask;
		}

		public async Task LogInfo(string message)
		{
			await WriteLogAsync(message.ToString());
		}
	}
}
