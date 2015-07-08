using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StockSimulator.Core.BuySellConditions;

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
		/// <param name="sizeOfOrder">Amount of money to place in this order</param>
		/// <param name="dependentIndicators">List of all the dependent indicators</param>
		/// <param name="buyConditions">All the buy conditions that must be met to fill the order</param>
		/// <param name="sellConditions">Any of the sell conditions trigger a sell</param>
		/// <param name="extraOrderInfo">The extra order info from the substrategy</param>
		public MainStrategyOrder(
			List<StrategyStatistics> stats, 
			double type, 
			TickerData tickerData, 
			string fromStrategyName, 
			int currentBar,
			double sizeOfOrder,
			List<string> dependentIndicators,
			List<BuyCondition> buyConditions,
			List<SellCondition> sellConditions,
			Dictionary<string, object> extraOrderInfo)
			: base(type, tickerData, fromStrategyName, currentBar, sizeOfOrder, dependentIndicators, buyConditions, sellConditions)
		{
			Statistics = stats;
			ExtraInfo = extraOrderInfo;
		}
	}
}
