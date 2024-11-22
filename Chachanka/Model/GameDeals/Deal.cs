using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chachanka.Model.GameDeals
{
	public class Deal
	{
		public required string internalName { get; set; }
		public required string title { get; set; }
		public string? metacriticLink { get; set; }
		public required string dealID { get; set; }
		public required string storeID { get; set; }
		public required string gameID { get; set; }
		public required string salePrice { get; set; }
		public required string normalPrice { get; set; }
		public required string isOnSale { get; set; }
		public required string savings { get; set; }
		public required string metacriticScore { get; set; }
		public string? steamRatingText { get; set; }
		public required string steamRatingPercent { get; set; }
		public required string steamRatingCount { get; set; }
		public string? steamAppID { get; set; }
		public required uint releaseDate { get; set; }
		public required uint lastChange { get; set; }
		public required string dealRating { get; set; }
		public required string thumb { get; set; }
	}
}
