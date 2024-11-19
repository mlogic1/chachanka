using Discord;

namespace chachanka.Interface
{
	internal interface ILoggingService
	{
		public Task LogInfo(string message);
	}
}
