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
	public class TickerExchangePair
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

		/// <summary>
		/// Combines the exchange and ticker for the string name.
		/// </summary>
		/// <returns>String of the exchange plus the ticker.</returns>
		public override string ToString()
		{
			return Exchange + ':' + Ticker;
		}

		/// <summary>
		/// Gets the hash of the exchange and ticker. This can be used for storing 
		/// the name in a dictionary.
		/// </summary>
		/// <returns>Hash of the exchange plus the ticker</returns>
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}
	}
}
