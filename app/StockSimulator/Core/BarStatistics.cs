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
	public class OrderSuggestion
	{
		public double HighestPercent { get; set; }
		public double HighestGain { get; set; }
		public string HighestStrategyName { get; set; }
		public double StrategyOrderType { get; set; }
		public double SizeOfOrder { get; set; }
		public List<Indicator> DependentIndicators { get; set; }
		public List<StrategyStatistics> Statistics { get; set; }
		public List<BuyCondition> BuyConditions { get; set; }
		public List<SellCondition> SellConditions { get; set; }

		public OrderSuggestion()
		{
			HighestPercent = 0.0;
			HighestGain = 0.0;
			HighestStrategyName = "None";
			StrategyOrderType = Order.OrderType.Long;
			SizeOfOrder = 0.0;
			DependentIndicators = new List<Indicator>();
			Statistics = new List<StrategyStatistics>();
			BuyConditions = new List<BuyCondition>();
			SellConditions = new List<SellCondition>();
		}

		public OrderSuggestion(
			double percent,
			double gain,
			string name,
			double orderType,
			double sizeOfOrder,
			List<Indicator> dependentIndicators,
			List<StrategyStatistics> statistics,
			List<BuyCondition> buyConditions,
			List<SellCondition> sellConditions)
		{
			HighestPercent = percent;
			HighestGain = gain;
			HighestStrategyName = name;
			StrategyOrderType = orderType;
			SizeOfOrder = sizeOfOrder;
			DependentIndicators = dependentIndicators;
			Statistics = statistics;
			BuyConditions = buyConditions;
			SellConditions = sellConditions;
		}
	}
}
