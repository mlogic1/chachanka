using Discord;

namespace Chachanka.Interface
{
	internal interface ILoggingService
	{
		public Task LogInfo(string message);
	}
}
