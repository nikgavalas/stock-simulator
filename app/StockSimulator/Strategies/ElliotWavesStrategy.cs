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
	/// </summary>
	public class ElliotWavesStrategy : RootSubStrategy
	{
		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		public ElliotWavesStrategy(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				new ElliotWaves(tickerData)
			};
		}

		/// <summary>
		/// Returns the name of this strategy.
		/// </summary>
		/// <returns>The name of this strategy</returns>
		public override string ToString()
		{
			return "ElliotWavesStrategy";
		}

		/// <summary>
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar < 2)
			{
				return;
			}

			ElliotWaves waves = (ElliotWaves)_dependents[0];
			double buyDirection = 0.0;
			string foundStrategyName = "";

			// See if we just started the 5th wave
			if (waves.FifthWaveValue[currentBar - 1] == 0.0 && waves.FifthWaveValue[currentBar] > 0.0)
			{
				buyDirection = waves.FifthWaveDirection[currentBar];
				foundStrategyName = buyDirection > 0.0 ? "BullElliotWavesStrategy" : "BearElliotWavesStrategy";
			}

			if (buyDirection != 0.0)
			{
				// TODO: use the 1 bar trailing high/low entry conditions.
				List<BuyCondition> buyConditions = new List<BuyCondition>()
				{
					//new AboveSetupBarBuyCondition(Simulator.Config.BressertMaxBarsToFill)
					new MarketBuyCondition()
				};

				// Always have a max time in market and an absolute stop for sell conditions.
				List<SellCondition> sellConditions = new List<SellCondition>()
				{
					new StopSellCondition(0.05),
					new MaxLengthSellCondition(5),
				};

				List<Indicator> dependentIndicators = GetDependentIndicators();

				Order placedOrder = EnterOrder(foundStrategyName, currentBar, buyDirection, 10000,
					dependentIndicators, buyConditions, sellConditions);

				if (placedOrder != null)
				{
					// Get things like win/loss percent up to the point this order was started.
					StrategyStatistics orderStats = Simulator.Orders.GetStrategyStatistics(placedOrder.StrategyName,
						placedOrder.Type,
						placedOrder.Ticker.TickerAndExchange,
						currentBar,
						Simulator.Config.MaxLookBackBars);

					Bars[currentBar] = new OrderSuggestion(
						100.0,
						foundStrategyName,
						buyDirection,
						10000,
						dependentIndicators,
						new List<StrategyStatistics>() { orderStats },
						buyConditions,
						sellConditions);
				}
			}
		}

	
	}
}
