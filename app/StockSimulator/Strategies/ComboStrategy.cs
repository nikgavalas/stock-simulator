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
	class ComboStrategy : RootSubStrategy
	{
		protected int _minComboSize;
		protected int _maxComboSize;
		protected int _maxBarsOpen;
		protected int _comboLeewayBars;
		protected double _sizeOfOrder;
		protected double _stopPercent;
		protected string _namePrefix;

		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		/// <param name="factory">Factory for creating dependents</param>
		public ComboStrategy(TickerData tickerData, RunnableFactory factory) 
			: base(tickerData, factory)
		{
			_minComboSize = Simulator.Config.ComboMinComboSize;
			_maxComboSize = Simulator.Config.ComboMaxComboSize;
			_maxBarsOpen = Simulator.Config.ComboMaxBarsOpen;
			_comboLeewayBars = Simulator.Config.ComboLeewayBars;
			_sizeOfOrder = Simulator.Config.ComboSizeOfOrder;
			_stopPercent = Simulator.Config.ComboStopPercent;
			_namePrefix = "";
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
					"BullDojiFound",
					"BearDojiFound",
					"HammerFound",
					"BullKeltnerExtended",
					"BullMacdCrossover",
					"BullMacdMomentum",
					"BullMomentumCrossover",
					"MorningStarFound",
					"PiercingLineFound",
					"RisingThreeMethodsFound",
					"BullRsiCrossover",
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
					"BullKeltnerExtendedPredicted",
					"BullMacdCrossoverPredicted",
					"BullMomentumCrossoverPredicted",
					"BullPriceOscillatorPredicted",
					"BullRsiCrossoverPredicted",
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
					"BearKeltnerExtended",
					"BearMacdMomentum",
					"BearMacdCrossover",
					"BearMomentumCrossover",
					"BearRsiCrossover",
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
					"BearKeltnerExtendedPredicted",
					"BearMacdCrossoverPredicted",
					"BearMomentumCrossoverPredicted",
					"BearPriceOscillatorPredicted",
					"BearRsiCrossoverPredicted",
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
			return "ComboStrategy";
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

			double highestWinPercent = 0.0;
			string highestName = "None";
			double highestOrderType = Order.OrderType.Long;
			StrategyStatistics highestStats = null;
			List<BuyCondition> highestBuyConditions = null;
			List<SellCondition> highestSellConditions = null;

			// Bull and bear strategies can't combo with each other but we still want
			// to compare them side by side to find our what combo is the best.
			// So append all the bear combos to the combo list so they can be evaluated too.
			List<List<Strategy>> combos = Data.HigherTimeframeMomentum[currentBar] == Order.OrderType.Long ?
				GetComboList(currentBar, Order.OrderType.Long) : GetComboList(currentBar, Order.OrderType.Short);

			// Place orders for all the combos.
			List<StrategyStatistics> stats = new List<StrategyStatistics>();
			for (int i = 0; i < combos.Count; i++)
			{
				List<Strategy> comboList = combos[i];

				// Ignore combos greater than a max amount.
				if (comboList.Count > _maxComboSize || comboList.Count < _minComboSize)
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

				List<BuyCondition> buyConditions = GetBuyConditions();
				List<SellCondition> sellConditions = GetSellConditions(comboList);

				// Now that the name of the strategy is found, enter the order.
				Order placedOrder = EnterOrder(_namePrefix + comboName, currentBar, comboList[0].OrderType, _sizeOfOrder,
					dependentIndicators, buyConditions, sellConditions);

				if (placedOrder != null)
				{
					// Get things like win/loss percent up to the point this order was started.
					StrategyStatistics orderStats = Simulator.Orders.GetStrategyStatistics(placedOrder.StrategyName,
						placedOrder.Type,
						placedOrder.Ticker.TickerAndExchange,
						currentBar,
						Simulator.Config.MaxLookBackBars);
					stats.Add(orderStats);

					// For each combo we want to find out the winning % and the gain
					// for it and save those values for the bar.
					if (orderStats.WinPercent > highestWinPercent)
					{
						highestWinPercent = orderStats.WinPercent;
						highestName = orderStats.StrategyName;
						highestOrderType = orderStats.StrategyOrderType;
						highestStats = orderStats;
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
				_sizeOfOrder,
				stats,
				highestBuyConditions,
				highestSellConditions);
		}

		/// <summary>
		/// Returns a list of buy conditions.
		/// </summary>
		/// <returns>List of conditions that trigger a buy</returns>
		protected virtual List<BuyCondition> GetBuyConditions()
		{
			List<BuyCondition> buyConditions = new List<BuyCondition>()
			{
				new MarketBuyCondition()
			};

			return buyConditions;
		}

		/// <summary>
		/// Returns a list of sell conditions for the strategies that were found.
		/// </summary>
		/// <param name="strategies">List of strategies found</param>
		/// <returns>List of sell conditions that will trigger a sell</returns>
		protected virtual List<SellCondition> GetSellConditions(List<Strategy> strategies)
		{
			List<SellCondition> conditions = new List<SellCondition>();

			// Always have a max time in market and an absolute stop for sell conditions.
			conditions.Add(new StopSellCondition(_stopPercent));
			conditions.Add(new MaxLengthSellCondition(_maxBarsOpen));

			// Sell when any opposite strategy is found. So loop through all the strategies
			// that we involved with buying this order and find their counterparts. Then 
			// Add a sell condition for those.
			for (int i = 0; i < strategies.Count; i++)
			{
				Strategy s = strategies[i];
				string strategyTypeName = s.OrderType == Order.OrderType.Long ? "Bull" : "Bear";
				string oppositeTypeStrategyName = s.OrderType == Order.OrderType.Long ? "Bear" : "Bull";
				string currentStrategyName = s.ToString();

				// Some strategies don't have opposites. So we'll just not add it as a sell condition.
				if (currentStrategyName.StartsWith(strategyTypeName))
				{
					string oppositeStrategyName = s.ToString().Replace(strategyTypeName, oppositeTypeStrategyName);
					Strategy oppositeStrategy = (Strategy)_factory.GetRunnable(oppositeStrategyName);
					conditions.Add(new StrategyFoundSellCondition(oppositeStrategy));
				}
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
					for (int j = 0; j < _comboLeewayBars + 1; j++)
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
			// Min and max combo sizes are filtered elsewhere.
			ComboSet<Strategy> comboSet = new ComboSet<Strategy>(foundStrategies);
			List<List<Strategy>> combos = comboSet.GetSet(1);
			return combos;
		}
	}
}
