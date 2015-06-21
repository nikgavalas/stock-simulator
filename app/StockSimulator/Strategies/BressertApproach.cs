using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;
using StockSimulator.Core.BuySellConditions;
using StockSimulator.Indicators;

namespace StockSimulator.Strategies
{
	/// <summary>
	/// Strategy that takes stores the best sub strategies for each day along with each ones statistcis
	/// to the day it was found.
	/// </summary>
	public class BressertApproach : RootSubStrategy
	{
		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		/// <param name="factory">Factory for creating dependents</param>
		public BressertApproach(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"BullBressertDss",
					"BearBressertDss",
					"BressertTimingBands" // For the cycle length
				};

				return deps;
			}
		}

		/// <summary>
		/// Returns the name of this strategy.
		/// </summary>
		/// <returns>The name of this strategy</returns>
		public override string ToString()
		{
			return "BressertApproach";
		}

		/// <summary>
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			BullBressertDss bullStrategy = (BullBressertDss)Dependents[0];
			BearBressertDss bearStrategy = (BearBressertDss)Dependents[1];

			Strategy foundStrategy = null;
			Strategy oppositeStrategy = null;

			// See if any setup bars are found.
			if (bullStrategy.WasFound[currentBar] && Data.HigherTimeframeMomentum[currentBar] == Order.OrderType.Long)
			{
				foundStrategy = bullStrategy;
				oppositeStrategy = bearStrategy;
			}
			else if (bearStrategy.WasFound[currentBar] && Data.HigherTimeframeMomentum[currentBar] == Order.OrderType.Short)
			{
				foundStrategy = bearStrategy;
				oppositeStrategy = bullStrategy;
			}

			if (foundStrategy != null)
			{
				// TODO: use the 1 bar trailing high/low entry conditions.
				List<BuyCondition> buyConditions = new List<BuyCondition>()
				{
					new AboveSetupBarBuyCondition(Simulator.Config.BressertMaxBarsToFill)
				};

				// Always have a max time in market and an absolute stop for sell conditions.
				BressertTimingBands timingBands = (BressertTimingBands)Dependents[2];
				List<SellCondition> sellConditions = new List<SellCondition>()
				{
					new StopSetupBarLowSellCondition(),
					new MaxLengthSellCondition(foundStrategy == bullStrategy ? timingBands.LowCycleAvg[currentBar] : timingBands.HighCycleAvg[currentBar]),
					new StrategyFoundSellCondition(oppositeStrategy),
				};

				List<string> dependentIndicators = foundStrategy.GetDependentIndicatorNames();
				Order placedOrder = EnterOrder(foundStrategy.ToString(), currentBar, foundStrategy.OrderType, Simulator.Config.BressertSizeOfOrder,
					dependentIndicators, buyConditions, sellConditions);
			}

		}
	}
}
