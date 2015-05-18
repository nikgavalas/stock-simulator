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
	/// Tracks the stats of a strategy for a single ticker.
	/// Very similar to StrategyStatistics except it serializes the orders. 
	/// </summary>
	public class StrategyTickerPairStatistics : StatisticsCalculator
	{
		[JsonProperty("name")]
		public string StrategyName { get; set; }

		[JsonProperty("indicators")]
		public List<string> Indicators { get; set; }

		[JsonProperty("orders")]
		public override List<Order> Orders { get; set; }

		
		/////////////////// Ignored /////////////////////
		
		public string TickerName { get; set; }

		/// <summary>
		/// Just initialize the class.
		/// </summary>
		/// <param name="strategyName">The strategy these statistics are for</param>
		/// <param name="tickerName">Name of the ticker this strategy is working with</param>
		public StrategyTickerPairStatistics(string strategyName, string tickerName, List<string> dependentIndicatorNames)
			: base()
		{
			TickerName = tickerName;
			StrategyName = strategyName;
			Orders = new List<Order>();
			Indicators = dependentIndicatorNames; 
		}
	}
}
