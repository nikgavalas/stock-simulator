using StockSimulator.Core;
using StockSimulator.Core.BuySellConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Strategies
{
	class BressertComboStrategy : ComboStrategy
	{
		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		/// <param name="factory">Factory for creating dependents</param>
		public BressertComboStrategy(TickerData tickerData, RunnableFactory factory) 
			: base(tickerData, factory)
		{
			_minComboSize = Simulator.Config.BressertComboMinComboSize;
			_maxComboSize = Simulator.Config.BressertComboMaxComboSize;
			_maxBarsOpen = Simulator.Config.BressertComboMaxBarsOpen;
			_comboLeewayBars = Simulator.Config.BressertComboLeewayBars;
			_minPercentForBuy = Simulator.Config.BressertComboPercentForBuy;
			_sizeOfOrder = Simulator.Config.BressertComboSizeOfOrder;
			_stopPercent = Simulator.Config.BressertComboStopPercent;
			_namePrefix = "BC-";
		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					// Bull strategies
					"BullBressertDss10",
					"BullBressertDss5",
					"BullRsi3m3",

					// Bear strategies
					"BearBressertDss10",
					"BearBressertDss5",
					"BearRsi3m3"
				};

				return deps;
			}
		}

		/// <summary>
		/// Returns a list of buy conditions.
		/// </summary>
		/// <returns>List of conditions that trigger a buy</returns>
		//protected override List<BuyCondition> GetBuyConditions()
		//{
		//	// Use the setup bar trailing high/low entry conditions.
		//	List<BuyCondition> buyConditions = new List<BuyCondition>()
		//	{
		//		// TODO: move this config var to the right place.
		//		new AboveSetupBarBuyCondition(Simulator.Config.BressertMaxBarsToFill)
		//	};

		//	return buyConditions;
		//}

		/// <summary>
		/// Returns a list of sell conditions for the strategies that were found.
		/// </summary>
		/// <param name="strategies">List of strategies found</param>
		/// <returns>List of sell conditions that will trigger a sell</returns>
		//protected override List<SellCondition> GetSellConditions(List<Strategy> strategies)
		//{
		//	List<SellCondition> conditions = new List<SellCondition>();

		//	// Always have a max time in market and an absolute stop for sell conditions.
		//	conditions.Add(new StopSetupBarLowSellCondition());
		//	conditions.Add(new MaxLengthSellCondition(_maxBarsOpen));

		//	// Sell when any opposite strategy is found. So loop through all the strategies
		//	// that we involved with buying this order and find their counterparts. Then 
		//	// Add a sell condition for those.
		//	for (int i = 0; i < strategies.Count; i++)
		//	{
		//		Strategy s = strategies[i];
		//		string strategyTypeName = s.OrderType == Order.OrderType.Long ? "Bull" : "Bear";
		//		string oppositeTypeStrategyName = s.OrderType == Order.OrderType.Long ? "Bear" : "Bull";
		//		string currentStrategyName = s.ToString();

		//		// Some strategies don't have opposites. So we'll just not add it as a sell condition.
		//		if (currentStrategyName.StartsWith(strategyTypeName))
		//		{
		//			string oppositeStrategyName = s.ToString().Replace(strategyTypeName, oppositeTypeStrategyName);
		//			Strategy oppositeStrategy = (Strategy)_factory.GetRunnable(oppositeStrategyName);
		//			conditions.Add(new StrategyFoundSellCondition(oppositeStrategy));
		//		}
		//	}

		//	return conditions;
		//}
	
	}
}
