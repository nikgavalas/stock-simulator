using StockSimulator.Core.BuySellConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// For each bar the percent of the highest strategy that was found and 
	/// a list of all other percents for the other strategies that weren't as high.
	/// </summary>
	public class BarStatistics
	{
		public double HighestPercent { get; set; }
		public string HighestStrategyName { get; set; }
		public double StrategyOrderType { get; set; }
		public List<StrategyStatistics> Statistics { get; set; }
		public List<BuyCondition> BuyConditions { get; set; }
		public List<SellCondition> SellConditions { get; set; }

		public BarStatistics()
		{
			HighestPercent = 0.0;
			HighestStrategyName = "None";
			StrategyOrderType = Order.OrderType.Long;
			Statistics = new List<StrategyStatistics>();
			BuyConditions = new List<BuyCondition>();
			SellConditions = new List<SellCondition>();
		}

		public BarStatistics(double percent, string name, double orderType, List<StrategyStatistics> statistics)
		{
			HighestPercent = percent;
			HighestStrategyName = name;
			StrategyOrderType = orderType;
			Statistics = statistics;
			BuyConditions = new List<BuyCondition>();
			SellConditions = new List<SellCondition>();
		}
	}
}
