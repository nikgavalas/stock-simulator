using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StockSimulator.Core
{
	class MainStrategyOrder : Order
	{
		/// <summary>
		/// Holds the stats for all the strategies that were involved in making this purchase.
		/// </summary>
		[JsonProperty("strategies")]
		public List<StrategyStatistics> Statistics { get; set; }

		/// <summary>
		/// Only difference is it saves the strategy stats for this order.
		/// </summary>
		/// <param name="stats">Collection of strategy statistics</param>
		/// <param name="type">Order type (long or short)</param>
		/// <param name="tickerData">Ticker that this order was placed on</param>
		/// <param name="fromStrategyName">The name of the strategy that placed this order</param>
		/// <param name="currentBar">Current bar that the order was placed</param>
		public MainStrategyOrder(List<StrategyStatistics> stats, Order.OrderType type, TickerData tickerData, string fromStrategyName, int currentBar)
			: base(type, tickerData, fromStrategyName, currentBar)
		{
			Statistics = stats;
		}
	}
}
