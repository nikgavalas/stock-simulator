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
			return Ticker + '-' + Exchange;
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

		/// <summary>
		/// Checks if two tickers are equal.
		/// </summary>
		/// <param name="a">First ticker to compare</param>
		/// <param name="b">Second ticker to compare</param>
		/// <returns>True if the two tickers are equal</returns>
		public static bool operator ==(TickerExchangePair a, TickerExchangePair b)
		{
			if (Object.ReferenceEquals(a, b))
			{
				return true;
			}

			if (((object)a == null) || ((object)b == null))
			{
				return false;
			}

			return a.Ticker == b.Ticker && a.Exchange == b.Exchange;
		}

		/// <summary>
		/// Check if the object is equal to another.
		/// </summary>
		/// <param name="obj">Object to compare</param>
		/// <returns>True if the object passed in is equal to this object</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			TickerExchangePair ticker = obj as TickerExchangePair;
			if ((object)ticker == null)
			{
				return false;
			}

			return ticker.Ticker == Ticker && ticker.Exchange == Exchange;
		}

		/// <summary>
		/// Check if the two tickers are not equal.
		/// </summary>
		/// <param name="a">First ticker to compare</param>
		/// <param name="b">Second ticker to compare</param>
		/// <returns>True if the two tickers are not equal</returns>
		public static bool operator !=(TickerExchangePair a, TickerExchangePair b)
		{
			return !(a == b);
		}
	}
}
