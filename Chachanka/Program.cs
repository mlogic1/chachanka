// NTfHgUWSRw8W3lQVR_1R3ceFKQlpqYap

using chachanka.Interface;
using chachanka.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

static void ConfigureServices(IServiceCollection services)
{
	// Configure environment
	string[] RequiredEnvVars = new string[]
	{
		"BOTToken",
		"ChachankaDB"
	};

	bool dockerEnv = Environment.GetEnvironmentVariable("DOCKERIZED") != null;

	var configurationBuilder = new ConfigurationBuilder()
	.SetBasePath(AppContext.BaseDirectory);
	if (!dockerEnv)
	{
		// not running in docker: use appsettings (or appsettings.Development)
		configurationBuilder.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);

		Console.WriteLine("Not running in docker environment");
	}
	configurationBuilder.AddEnvironmentVariables();

	var configuration = configurationBuilder.Build();
	if (dockerEnv)
	{
		Console.WriteLine("Running in docker environment");
	}
	// copy over docker variables to the configuration
	foreach (string envVar in RequiredEnvVars)
	{
		if (Environment.GetEnvironmentVariable(envVar) == null)
		{
			Console.WriteLine($"Missing environment variable: '{envVar}'");
			Environment.Exit(-1);
		}
	}

	services.AddSingleton<IConfiguration>(configuration);
	services.AddSingleton<DiscordHandleService>();
	services.AddSingleton<ILoggingService, ConsoleLoggerService>();
	services.AddSingleton<DBService>();
	services.AddSingleton<GameDealsService>();
	services.AddSingleton<CronBgService>();
}

static async Task ProgramMainAsync()
{
	// Configure all services
	ServiceCollection servColl = new ServiceCollection();
	ConfigureServices(servColl);

	ServiceProvider serviceProvider = servColl.BuildServiceProvider();

	DBService? dbService = serviceProvider.GetService<DBService>();

	DiscordHandleService? discordHandle = serviceProvider.GetService<DiscordHandleService>();

	if (discordHandle == null)
	{
		Environment.Exit(-1);
	}

	await discordHandle.StartService();

	GameDealsService? gameDealService = serviceProvider.GetService<GameDealsService>();
	if (gameDealService != null)
	{
		await gameDealService.RefreshStoreList();
	}

	CronBgService? cronBgService = serviceProvider.GetService<CronBgService>();

	if (cronBgService != null)
	{
		cronBgService.InitService();
	}
	else
	{
		Console.WriteLine("CronBG is not ok");
		Environment.Exit(-1);
	}

	await Task.Delay(-1);

}

await ProgramMainAsync();