using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chachanka.Model.GameDeals
{
	public class Store
	{
		public required string storeID { get; set; }
		public required string storeName { get; set; }
		public required int isActive { get; set; }
	}
}
