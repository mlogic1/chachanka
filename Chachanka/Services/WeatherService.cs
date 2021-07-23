using Chachanka.Utility;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace Chachanka.Services
{
	public class WeatherService
	{
		const string weatherAPI = @"https://prognoza.hr/regije_danas.xml";
		private readonly ConsoleWriterService _consoleWriterService;

		private class WeatherReport
		{
			public string Datum { get; set; }
			public string Istocna { get; set; }
			public string Sredisnja { get; set; }
			public string SjeverniJadran { get; set; }
			public string Gorska { get; set; }
			public string Dalmacija { get; set; }
			public string Istra { get; set; }
		}

		public WeatherService(IServiceProvider services)
		{
			_consoleWriterService = services.GetRequiredService<ConsoleWriterService>();
		}

		public async Task ReportWeather(ISocketMessageChannel channel)
		{
			try
			{
				WeatherReport weatherResult = await GetWeatherData();

				var embed = new EmbedBuilder
				{
					Title = "Vremenska prognoza",
					Description = weatherResult.Datum
				};
				embed.AddField("Istočna", weatherResult.Istocna)
					.AddField("Središnja", weatherResult.Sredisnja)
					.AddField("Sjeverni Jadran", weatherResult.SjeverniJadran)
					.AddField("Gorska", weatherResult.Gorska)
					.AddField("Dalmacija", weatherResult.Dalmacija)
					.AddField("Istra", weatherResult.Istra)
					.WithFooter(footer => footer.Text = "Izvor podataka: DHMZ")
					.WithThumbnailUrl("https://emojipedia-us.s3.amazonaws.com/source/skype/289/sun-behind-small-cloud_1f324-fe0f.png");

				await channel.SendMessageAsync(embed: embed.Build());
			}
			catch(Exception ex)
			{
				await _consoleWriterService.WriteLogAsync("Something went wrong fetching weather data: " + ex.Message.ToString());
			}
		}

		private Task<WeatherReport> GetWeatherData()
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(weatherAPI);
			request.AutomaticDecompression = DecompressionMethods.GZip;

			string resultContent = string.Empty;
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw new WebException("HttpStatusCode is not 200");
				}
				resultContent = reader.ReadToEnd();
			}

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(resultContent);

			XmlElement element = xmlDoc["regije_danas"];

			return Task.FromResult(new WeatherReport
			{
				Datum = element["datum"].InnerXml.ToString(),
				Istocna = element["istocna"].InnerXml.ToString(),
				Sredisnja = element["sredisnja"].InnerXml.ToString(),
				SjeverniJadran = element["sjjadran"].InnerXml.ToString(),
				Gorska = element["gorska"].InnerXml.ToString(),
				Dalmacija = element["dalmacija"].InnerXml.ToString(),
				Istra = element["istra"].InnerXml.ToString()
			});
		}
	}
}
