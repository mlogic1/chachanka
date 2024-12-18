﻿using System.Data.SQLite;
using chachanka.Interface;
using chachanka.Model.GameDeals;
using Microsoft.Extensions.Configuration;

namespace chachanka.Services
{
	internal class DBService
	{
		private readonly ILoggingService _logger;
		private SQLiteConnection _sqliteConn;

		private readonly IConfiguration _configuration;

		public DBService(ILoggingService loggingService, IConfiguration configuration)
		{
			_logger = loggingService;
			_configuration = configuration;

			string? chachankadb = _configuration["ChachankaDB"];
			string connStr = "";
			if (chachankadb == null)
			{
				connStr = "/tmp/chanka.db"; // default to temp
				_logger.LogInfo("ChachankaDB defaulting to a temp file");
			}
			else
			{
				connStr = $"Data Source={chachankadb}";
			}
			_sqliteConn = new SQLiteConnection(connStr);

			_sqliteConn.Open();

			CheckDBSetup();
		}

		~DBService()
		{
			_sqliteConn.Close();
		}

		private void CheckDBSetup()
		{
			if (!TableExists("SeenDeals"))
			{
				string createSeenDeals = @"CREATE TABLE SeenDeals(
											internalName TEXT,
											title TEXT,
											metacriticLink TEXT,
											dealID TEXT,
											storeID TEXT,
											gameID TEXT,
											salePrice TEXT,
											normalPrice TEXT,
											isOnSale TEXT,
											savings TEXT,
											metacriticScore TEXT,
											steamRatingText TEXT,
											steamRatingPercent TEXT,
											steamRatingCount TEXT,
											steamAppID TEXT,
											releaseDate INT,
											lastChange INT,
											dealRating TEXT,
											thumb TEXT
											);";
				using (SQLiteCommand command = new SQLiteCommand(createSeenDeals, _sqliteConn))
				{
					command.ExecuteNonQuery();
				}
			}

			if (!TableExists("DealSubscribers"))
			{
				string createDealSubscribers = @"CREATE TABLE DealSubscribers(
											GuildId TEXT,
											ChannelId TEXT
											);";

				string indexCreate = @"CREATE UNIQUE INDEX idx_dealsubsribers_gid_cid on DealSubscribers (GuildId, ChannelId);";
				using (SQLiteCommand command = new SQLiteCommand(createDealSubscribers, _sqliteConn))
				{
					command.ExecuteNonQuery();
					command.CommandText = indexCreate;
					command.ExecuteNonQuery();
				}
			}
		}

		public async Task<bool> DealExistsInDb(Deal deal)
		{
			string query = $"SELECT count(*) FROM SeenDeals WHERE dealID='{deal.dealID}';";

			using (SQLiteCommand command = new SQLiteCommand(query, _sqliteConn))
			{
				int count = Convert.ToInt32(await command.ExecuteScalarAsync());
				return count > 0;
			}
		}

		private bool TableExists(string tableName)
		{
			string query = $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{tableName}';";

			using (SQLiteCommand command = new SQLiteCommand(query, _sqliteConn))
			{
				int count = Convert.ToInt32(command.ExecuteScalar());
				return count > 0;
			}
		}

		public async Task<List<string>> GetAllDeals()
		{
			string selectQuery = "SELECT dealId FROM SeenDeals;";
			List<string> seenDeals = new List<string>();
			using (SQLiteCommand command = new SQLiteCommand(selectQuery, _sqliteConn))
			{
				using (var reader = await command.ExecuteReaderAsync())
				{
					while(await reader.ReadAsync())
					{
						seenDeals.Add(reader.GetString(0));
					}
				}
			}
			return seenDeals;
		}

		public async Task<List<DealSubscriber>> GetAllDealSubscribers()
		{
			List<DealSubscriber> subs = new List<DealSubscriber>();
			string selectQuery = "SELECT GuildId, ChannelId FROM DealSubscribers;";

			using (SQLiteCommand command = new SQLiteCommand(selectQuery, _sqliteConn))
			{
				using (var reader = await command.ExecuteReaderAsync())
				{
					while(await reader.ReadAsync())
					{
						subs.Add(new DealSubscriber()
						{
							GuildId = reader.GetString(0),
							ChannelId = reader.GetString(1)
						});
					}
				}
			}

			return subs;
		}

		public async Task<bool> StoreDealSubscriber(DealSubscriber sub)
		{
			try
			{
				string insertQuery = "INSERT INTO DealSubscribers(GuildId, ChannelId) VALUES(@p_guildid, @p_channelid)";
				using (SQLiteCommand command = new SQLiteCommand(insertQuery, _sqliteConn))
				{
					command.Parameters.AddWithValue("@p_guildid", sub.GuildId);
					command.Parameters.AddWithValue("@p_channelid", sub.ChannelId);

					int result = await command.ExecuteNonQueryAsync();

					if (result < 1)
					{
						await _logger.LogInfo("[DBService] Inserted 0 rows to DealSubscribers");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogInfo(ex.Message);
				return false;
			}
			return true;
		}

		public async Task<bool> DeleteDealSubscriber(DealSubscriber sub)
		{
			try
			{
				string insertQuery = "DELETE FROM DealSubscribers WHERE GuildId = @p_guildid AND ChannelId = @p_channelid;";
				using (SQLiteCommand command = new SQLiteCommand(insertQuery, _sqliteConn))
				{
					command.Parameters.AddWithValue("@p_guildid", sub.GuildId);
					command.Parameters.AddWithValue("@p_channelid", sub.ChannelId);

					int result = await command.ExecuteNonQueryAsync();

					if (result < 1)
					{
						await _logger.LogInfo("[DBService] Deleted 0 rows from DealSubscribers");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogInfo(ex.Message);
				return false;
			}

			return true;
		}

		public async Task StoreDeal(Deal deal)
		{
			try
			{
				string insertQuery = "INSERT INTO SeenDeals (internalName, title, metacriticLink, dealID, storeID, gameID, salePrice, normalPrice, isOnSale, savings, metacriticScore, steamRatingText, steamRatingPercent, steamRatingCount, steamAppID, releaseDate, lastChange, dealRating, thumb) VALUES (@p_internalName, @p_title, @p_metacriticLink, @p_dealID, @p_storeID, @p_gameID, @p_salePrice, @p_normalPrice, @p_isOnSale, @p_savings, @p_metacriticScore, @p_steamRatingText, @p_steamRatingPercent, @p_steamRatingCount, @p_steamAppID, @p_releaseDate, @p_lastChange, @p_dealRating, @p_thumb)";
				using (SQLiteCommand command = new SQLiteCommand(insertQuery, _sqliteConn))
				{
					command.Parameters.AddWithValue("@p_internalName", deal.internalName);
					command.Parameters.AddWithValue("@p_title", deal.title);
					command.Parameters.AddWithValue("@p_metacriticLink", deal.metacriticLink);
					command.Parameters.AddWithValue("@p_dealID", deal.dealID);
					command.Parameters.AddWithValue("@p_storeID", deal.storeID);
					command.Parameters.AddWithValue("@p_gameID", deal.gameID);
					command.Parameters.AddWithValue("@p_salePrice", deal.salePrice);
					command.Parameters.AddWithValue("@p_normalPrice", deal.normalPrice);
					command.Parameters.AddWithValue("@p_isOnSale", deal.isOnSale);
					command.Parameters.AddWithValue("@p_savings", deal.savings);
					command.Parameters.AddWithValue("@p_metacriticScore", deal.metacriticScore);
					command.Parameters.AddWithValue("@p_steamRatingText", deal.steamRatingText);
					command.Parameters.AddWithValue("@p_steamRatingPercent", deal.steamRatingPercent);
					command.Parameters.AddWithValue("@p_steamRatingCount", deal.steamRatingCount);
					command.Parameters.AddWithValue("@p_steamAppID", deal.steamAppID);
					command.Parameters.AddWithValue("@p_releaseDate", deal.releaseDate);
					command.Parameters.AddWithValue("@p_lastChange", deal.lastChange);
					command.Parameters.AddWithValue("@p_dealRating", deal.dealRating);
					command.Parameters.AddWithValue("@p_thumb", deal.thumb);
					int result = await command.ExecuteNonQueryAsync();

					if (result < 1)
					{
						await _logger.LogInfo("[DBService] Inserted 0 rows");
					}
				}
			}
			catch(System.Data.Common.DbException dbException)
			{
				await _logger.LogInfo($"[DBService] Fail insert SeenDeal: {dbException.Message}");
				if (dbException.StackTrace != null)
				{
					await _logger.LogInfo(dbException.StackTrace);
				}
			}

		}

		public async Task StoreDeals(List<Deal> deals)
		{
			foreach(Deal deal in deals)
			{
				await StoreDeal(deal);
			}
		}
	}
}
