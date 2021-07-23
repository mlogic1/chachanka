using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Chachanka.Utility
{
	public class ConsoleWriterService
	{
		private BlockingCollection<string> _messageQueue;

		public ConsoleWriterService()
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
	}
}
