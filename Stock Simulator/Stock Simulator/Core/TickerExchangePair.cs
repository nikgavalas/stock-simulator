using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// Simple class to hold a ticker name and the exchange it belongs to.
	/// </summary>
	class TickerExchangePair
	{
		/// <summary>
		/// Ticker name.
		/// </summary>
		public string Ticker { get; set; }
		
		/// <summary>
		/// Exchange the ticker belongs to.
		/// </summary>
		public string Exchange { get; set; }

		/// <summary>
		/// Constructor to initialize the properties.
		/// </summary>
		/// <param name="exchange">Exchange the ticker belongs to</param>
		/// <param name="tickerName">Name of the ticker</param>
		public TickerExchangePair(string exchange, string tickerName)
		{
			Ticker = tickerName;
			Exchange = exchange;
		}
	}
}
