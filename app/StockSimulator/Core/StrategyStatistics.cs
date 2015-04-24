using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StockSimulator.Core.JsonConverters;

namespace StockSimulator.Core
{
	/// <summary>
	/// Class to hold all the statistics for a strategy.
	/// </summary>
	public class StrategyStatistics : StatisticsCalculator
	{
		[JsonProperty("name")]
		public string StrategyName { get; set; }

		[JsonProperty("orderType")]
		public Order.OrderType StrategyOrderType { get; set; }

		/// <summary>
		/// Constructor that doesn't calculate the stats to be used with add order.
		/// </summary>
		/// <param name="strategyName">Name of the strategy</param>
		/// <param name="orderType">Type of the order this strategy is for</param>
		public StrategyStatistics(string strategyName, Order.OrderType orderType)
			: base()
		{
			StrategyName = strategyName;
			StrategyOrderType = orderType;
		}

	}
}
