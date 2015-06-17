using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core;
using StockSimulator.Core.BuySellConditions;

namespace StockSimulator.Strategies
{
	/// <summary>
	/// Returns the best performing combo of lots of substrategies
	/// for a given trading bar.
	/// </summary>
	class BestComboStrategy : RootSubStrategy
	{
		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		/// <param name="factory">Factory for creating dependents</param>
		public BestComboStrategy(TickerData tickerData, RunnableFactory factory) 
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
					"BullStochRsiCrossover",
					"ThreeWhiteSoldiersFound",
					"BullTrendStart",
					"BullTrixSignalCrossover",
					"BullTrixZeroCrossover",
					"UpsideTasukiGapFound",
					"BullWilliamsRCrossover",
					"BullEaseOfMovement",
					"BullPriceOscillator",
					"BullDmi",

					// Predicted bull strategies
					"BullCciCrossoverPredicted",
					"BullDmiPredicted",
					"BullEaseOfMovementPredicted",
					"BullKeltnerCloseAbovePredicted",
					"BullMacdCrossoverPredicted",
					"BullMomentumCrossoverPredicted",
					"BullPriceOscillatorPredicted",
					"BullRsiCrossover30Predicted",
					"BullSmaCrossoverPredicted",
					"BullStochasticsCrossoverPredicted",
					"BullStochasticsFastCrossoverPredicted",
					"BullStochRsiCrossoverPredicted",
					"BullTrixSignalCrossoverPredicted",
					"BullTrixZeroCrossoverPredicted",
					"BullWilliamsRCrossoverPredicted",

					// Bear strategies
					"BearBollingerExtended",
					"BearBeltHoldFound",
					"BearCciCrossover",
					"BearEngulfingFound",
					"BearHaramiFound",
					"BearHaramiCrossFound",
					"BearKeltnerCloseBelow",
					"BearMacdMomentum",
					"BearMacdCrossover",
					"BearMomentumCrossover",
					"BearRsiCrossover70",
					"BearSmaCrossover",
					"BearStochasticsCrossover",
					"BearStochasticsFastCrossover",
					"BearStochRsiCrossover",
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
					"UpsideGapTwoCrowsFound",
					"BearEaseOfMovement",
					"BearPriceOscillator",
					"BearDmi",

					// Predicted bear strategies
					"BearCciCrossoverPredicted",
					"BearDmiPredicted",
					"BearEaseOfMovementPredicted",
					"BearKeltnerCloseBelowPredicted",
					"BearMacdCrossoverPredicted",
					"BearMomentumCrossoverPredicted",
					"BearPriceOscillatorPredicted",
					"BearRsiCrossover70Predicted",
					"BearSmaCrossoverPredicted",
					"BearStochasticsCrossoverPredicted",
					"BearStochasticsFastCrossoverPredicted",
					"BearStochRsiCrossoverPredicted",
					"BearTrixSignalCrossoverPredicted",
					"BearTrixZeroCrossoverPredicted",
					"BearWilliamsRCrossoverPredicted",

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
			return "BestComboStrategy";
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

			double highestWinPercent = 0;
			string highestName = "None";
			double highestOrderType = Order.OrderType.Long;
			StrategyStatistics highestStats = null;
			List<BuyCondition> highestBuyConditions = null;
			List<SellCondition> highestSellConditions = null;

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
				if (comboList.Count > Simulator.Config.ComboMaxComboSize || comboList.Count < Simulator.Config.ComboMinComboSize)
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

				// Get all the buy and sell conditions for this order. Buy conditions are easy,
				// we just want to buy on the next frame at market price.
				List<BuyCondition> buyConditions = new List<BuyCondition>()
				{
					new MarketBuyCondition()
				};

				List<SellCondition> sellConditions = GetSellConditions(comboList);

				// Now that the name of the strategy is found, enter the order.
				Order placedOrder = EnterOrder(comboName, currentBar, comboList[0].OrderType, Simulator.Config.ComboSizeOfOrder,
					dependentIndicators, buyConditions, sellConditions);

				if (placedOrder != null)
				{
					// Get things like win/loss percent up to the point this order was started.
					stats.Add(Simulator.Orders.GetStrategyStatistics(placedOrder.StrategyName,
						placedOrder.Type,
						placedOrder.Ticker.TickerAndExchange,
						currentBar,
						Simulator.Config.MaxLookBackBars));

					// For each combo we want to find out the winning % and the gain
					// for it and save those values for the bar.
					if (stats[i].WinPercent > highestWinPercent)
					{
						highestWinPercent = stats[i].WinPercent;
						highestName = stats[i].StrategyName;
						highestOrderType = stats[i].StrategyOrderType;
						highestStats = stats[i];
						highestBuyConditions = buyConditions;
						highestSellConditions = sellConditions;
					}
				}
			}

			// Abbreviated output we only care about the strategy used to do the buy,
			// not all the ones that could have been found.
			if (Simulator.Config.UseAbbreviatedOutput == true)
			{
				stats = new List<StrategyStatistics>();
				stats.Add(highestStats);
			}

			Bars[currentBar] = new BarStatistics(
				highestWinPercent,
				highestName,
				highestOrderType,
				Simulator.Config.ComboSizeOfOrder,
				stats,
				highestBuyConditions,
				highestSellConditions);
		}

		/// <summary>
		/// Returns a list of sell conditions for the strategies that were found.
		/// </summary>
		/// <param name="strategies">List of strategies found</param>
		/// <returns>List of sell conditions that will trigger a sell</returns>
		private List<SellCondition> GetSellConditions(List<Strategy> strategies)
		{
			List<SellCondition> conditions = new List<SellCondition>();

			// Always have a max time in market and an absolute stop for sell conditions.
			conditions.Add(new StopSellCondition(Simulator.Config.ComboStopPercent));
			conditions.Add(new MaxLengthSellCondition(Simulator.Config.ComboMaxBarsOpen));

			// Sell when any opposite strategy is found. So loop through all the strategies
			// that we involved with buying this order and find their counterparts. Then 
			// Add a sell condition for those.
			for (int i = 0; i < strategies.Count; i++)
			{
				Strategy s = strategies[i];
				string strategyTypeName = s.OrderType == Order.OrderType.Long ? "Bull" : "Bear";
				string oppositeTypeStrategyName = s.OrderType == Order.OrderType.Long ? "Bear" : "Bull";

				string oppositeStrategyName = s.ToString().Replace(strategyTypeName, oppositeTypeStrategyName);
				Strategy oppositeStrategy = (Strategy)_factory.GetRunnable(oppositeStrategyName);
				conditions.Add(new StrategyFoundSellCondition(s));
			}

			return conditions;
		}

		/// <summary>
		/// Builds a list of all the possible combos found on this bar.
		/// </summary>
		/// <param name="currentBar">Current bar to search for combos</param>
		/// <param name="orderType">Type of strategies to combo together</param>
		/// <returns>List of all the combo lists for this bar</returns>
		private List<List<Strategy>> GetComboList(int currentBar, double orderType)
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
