using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;

namespace StockSimulator.Strategies
{
	/// <summary>
	/// Strategy that takes stores the best sub strategies for each day along with each ones statistcis
	/// to the day it was found.
	/// </summary>
	public class BestOfSubStrategies : Strategy
	{
		/// <summary>
		/// For each bar the percent of the highest strategy that was found and 
		/// a list of all other percents for the other strategies that weren't as high.
		/// </summary>
		public class BarStatistics
		{
			public double HighestPercent { get; set; }
			public string HighestStrategyName { get; set; }
			public int ComboSizeOfHighestStrategy { get; set; }
			public Order.OrderType StrategyOrderType { get; set; }
			public List<StrategyStatistics> Statistics { get; set; }

			public BarStatistics()
			{
				HighestPercent = 0.0;
				HighestStrategyName = "None";
				ComboSizeOfHighestStrategy = 0;
				StrategyOrderType = Order.OrderType.Long;
				Statistics = new List<StrategyStatistics>();
			}

			public BarStatistics(double percent, string name, int comboSize, Order.OrderType orderType, List<StrategyStatistics> statistics)
			{
				HighestPercent = percent;
				HighestStrategyName = name;
				ComboSizeOfHighestStrategy = comboSize;
				StrategyOrderType = orderType;
				Statistics = statistics;
			}
		}

		/// <summary>
		/// List of bar data.
		/// </summary>
		public List<BarStatistics> Bars { get; set; }

		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		/// <param name="factory">Factory for creating dependents</param>
		public BestOfSubStrategies(TickerData tickerData, RunnableFactory factory) 
			: base(tickerData, factory)
		{
			Bars = Enumerable.Repeat(new BarStatistics(), tickerData.NumBars).ToList();
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
					"BullBollingerExtended",
					"BullBeltHoldFound",
					"BullEngulfingFound",
					"BullHaramiFound",
					"BullHaramiCrossFound",
					"BullCciCrossover",
					"DojiFound",
					"BearDojiFound",
					"HammerFound",
					"BullKeltnerCloseAbove",
					"BullMacdCrossover",
					"BullMacdMomentum",
					"BullMomentumCrossover",
					"MorningStarFound",
					"PiercingLineFound",
					"RisingThreeMethodsFound",
					"BullRsiCrossover30",
					"BullSmaCrossover",
					"StickSandwitchFound",
					"BullStochasticsFastCrossover",
					"BullStochasticsCrossover",
					"BullStochRsiFound",
					"ThreeWhiteSoldiersFound",
					"BullTrendStart",
					"BullTrixSignalCrossover",
					"BullTrixZeroCrossover",
					"UpsideTasukiGapFound",
					"BullWilliamsRCrossover",

					// Bear strategies
					"BearBollingerExtended",
					"BearBeltHoldFound",
					"BearCciCrossover",
					"BearEngulfingFound",
					"BearHaramiFound",
					"BearHaramiCrossFound",
					"BearKeltnerCloseAbove",
					"BearMacdMomentum",
					"BearMacdCrossover",
					"BearMomentumCrossover",
					"BearRsiCrossover70",
					"BearSmaCrossover",
					"BearStochasticsCrossover",
					"BearStochasticsFastCrossover",
					"BearStochRsiFound",
					"BearTrendStart",
					"BearTrixSignalCrossover",
					"BearTrixZeroCrossover",
					"BearWilliamsRCrossover",
					"DarkCloudCoverFound",
					"DownsideTasukiGapFound",
					"EveningStarFound",
					"FallingThreeMethodsFound",
					"HangingManFound",
					"InvertedHammerFound",
					"ShootingStarFound",
					"ThreeBlackCrowsFound",
					"UpsideGapTwoCrowsFound"
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
			return "BestOfSubStrategies";
		}

		/// <summary>
		/// Sees which strategies were found on this far and places orders for all 
		/// the combos of those strategies. The value of this strategy is the best 
		/// strategy that was found on this bar based on the success of the history
		/// of that strategy.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			// Bull and bear strategies can't combo with each other but we still want
			// to compare them side by side to find our what combo is the best.
			// So append all the bear combos to the combo list so they can be evaluated too.
			List<List<Strategy>> combos = GetComboList(currentBar, Order.OrderType.Long);
			combos.AddRange(GetComboList(currentBar, Order.OrderType.Short));

			// Place orders for all the combos.
			List<StrategyStatistics> stats = new List<StrategyStatistics>();
			for (int i = 0; i < combos.Count; i++)
			{
				List<Strategy> comboList = combos[i];

				// Ignore combos greater than a max amount.
				if (comboList.Count > Simulator.Config.MaxComboSizeToBuy)
				{
					continue;
				}

				List<string> dependentIndicators = new List<string>();
				string comboName = "";
				for (int j = 0; j < comboList.Count; j++)
				{
					comboName += comboList[j].ToString();
					comboName += "-";

					// Keep track of the dependent indicators for this strategy.
					dependentIndicators.AddRange(comboList[j].GetDependentIndicatorNames());
				}

				// Trim off the last '-'
				if (comboList.Count > 0)
				{
					comboName = comboName.TrimEnd('-');
				}

				// Now that the name of the strategy is found, enter the order.
				Order placedOrder = EnterOrder(comboName, currentBar, comboList[0].OrderType, dependentIndicators);
				if (placedOrder != null)
				{
					stats.Add(placedOrder.StartStatistics);
				}
			}

			// For each combo we want to find out the winning % and the gain
			// for it and save those values for the bar.
			double highestWinPercent = 0;
			string highestName = "None";
			int comboSize = 0;
			Order.OrderType orderType = Order.OrderType.Long;
			for (int i = 0; i < stats.Count; i++)
			{
				if (stats[i].ProfitTargetPercent > highestWinPercent)
				{
					highestWinPercent = stats[i].ProfitTargetPercent;
					highestName = stats[i].StrategyName;
					comboSize = stats[i].StrategyName.Split('-').Length;
					orderType = stats[i].StrategyOrderType;
				}
			}

			Bars[currentBar] = new BarStatistics(highestWinPercent, highestName, comboSize, orderType, stats);
		}

		/// <summary>
		/// Builds a list of all the possible combos found on this bar.
		/// </summary>
		/// <param name="currentBar">Current bar to search for combos</param>
		/// <param name="orderType">Type of strategies to combo together</param>
		/// <returns>List of all the combo lists for this bar</returns>
		private List<List<Strategy>> GetComboList(int currentBar, Order.OrderType orderType)
		{
			// Get all the strategies that were found on today.
			List<Strategy> foundStrategies = new List<Strategy>();
			for (int i = 0; i < Dependents.Count; i++)
			{
				if (Dependents[i] is Strategy)
				{
					Strategy dependentStrategy = (Strategy)Dependents[i];

					// Ensure each dependent has the same order type.
					if (orderType != dependentStrategy.OrderType)
					{
						continue;
					}

					// Check here that the strategy order type matches
					// with the higher timeframe trend. Continue if it doesn't.
					if (Simulator.Config.UseHigherTimeframeSubstrategies == true && dependentStrategy.OrderType != dependentStrategy.Data.HigherTimeframeMomentum[currentBar])
					{
						continue;
					}

					// The logic here that searches the current bar
					// and back a few days to make the finding not so exact. It adds
					// for some more leeway when finding combos as finding multiple
					// strategies on exact bars doesn't happen as frequently.
					for (int j = 0; j < Simulator.Config.ComboLeewayBars + 1; j++)
					{
						int comboBar = currentBar - j;
						if (comboBar < 0)
						{
							comboBar = 0;
						}

						if (dependentStrategy.WasFound[comboBar])
						{
							foundStrategies.Add(dependentStrategy);
							break;
						}
					}
				}
			}

			// Create all the possible combos of these strategies.
			// TODO: If we have it where we are finding more than 5
			// strategies on a day we probably don't need to bother
			// with tracking more than that since the likelyhood
			// of finding a point all of those 5 strategies again is small.
			// So we might want to set a max.
			ComboSet<Strategy> comboSet = new ComboSet<Strategy>(foundStrategies);
			List<List<Strategy>> combos = comboSet.GetSet(1);
			return combos;
		}
	}
}
