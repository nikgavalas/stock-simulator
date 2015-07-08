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
	public class GavalasStrategy : RootSubStrategy
	{
		private class PriceZone
		{
			public double High { get; set; }
			public double Low { get; set; }
			public int NumberOfPoints { get; set; }
		}

		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		public GavalasStrategy(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				new GavalasZones(tickerData),
				new DtOscillator(tickerData) { PeriodRsi = 8, PeriodStoch = 5, PeriodSK = 3, PeriodSD = 3 },
				new GavalasHistogram(tickerData),
				new DmiAdx(tickerData),
				new AverageVolume(tickerData)
			};
		}

		/// <summary>
		/// Returns the name of this strategy.
		/// </summary>
		/// <returns>The name of this strategy</returns>
		public override string ToString()
		{
			return "GavalasStrategy";
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

			GavalasZones zones = (GavalasZones)_dependents[0];
			GavalasHistogram histogram = (GavalasHistogram)_dependents[2];

			double buyDirection = zones.BuyDirection[currentBar];
			string foundStrategyName = "";

			// See if we hit one of our buy zones.
			if (buyDirection != 0.0 && zones.DidBarTouchZone(Data.Low[currentBar], Data.High[currentBar], currentBar))
			{
				// Verify with the mechanical buy signal.
				DtOscillator dtosc = (DtOscillator)_dependents[1];
				if (buyDirection > 0.0 && Data.HigherTimeframeTrend[currentBar] > 0.0)
				{
					if (dtosc.SD[currentBar] <= 25.0 && dtosc.SK[currentBar] <= 25.0)
					{
						foundStrategyName = "BullGavalasStrategy";
					}
				}
				else if (buyDirection < 0.0 && Data.HigherTimeframeTrend[currentBar] < 0.0)
				{
					if (dtosc.SD[currentBar] >= 75.0 && dtosc.SK[currentBar] >= 75.0)
					{
						foundStrategyName = "BearGavalasStrategy";
					}
				}
			}

			if (foundStrategyName.Length > 0 && DoesPassFilters(currentBar))
			{
				List<Indicator> dependentIndicators = GetDependentIndicators();

				Order placedOrder = EnterOrder(foundStrategyName, currentBar, buyDirection, Simulator.Config.GavalasSizeOfOrder,
					dependentIndicators, GetBuyConditions(), GetSellConditions());

				if (placedOrder != null)
				{
					// Get things like win/loss percent up to the point this order was started.
					StrategyStatistics orderStats = Simulator.Orders.GetStrategyStatistics(placedOrder.StrategyName,
						placedOrder.Type,
						placedOrder.Ticker.TickerAndExchange,
						currentBar,
						Simulator.Config.MaxLookBackBars);

					AddExtraOrderInfo(placedOrder, currentBar);

					if (orderStats.WinPercent >= Simulator.Config.GavalasPercentForBuy)
					{
						Bars[currentBar] = new OrderSuggestion(
							orderStats.WinPercent,
							orderStats.Gain,
							foundStrategyName,
							buyDirection,
							Simulator.Config.GavalasSizeOfOrder,
							dependentIndicators,
							new List<StrategyStatistics>() { orderStats },
							GetBuyConditions(),
							GetSellConditions(),
							placedOrder.ExtraInfo);
					}
				}
			}
		}

		/// <summary>
		/// Returns the list of buy conditions for this strategy.
		/// </summary>
		/// <returns>List of buy conditions for this strategy</returns>
		private List<BuyCondition> GetBuyConditions()
		{
			return new List<BuyCondition>()
			{
				//new OneBarTrailingHighLow(5)
				new MarketBuyCondition()
			};
		}

		/// <summary>
		/// Returns the list of sell conditions for this strategy.
		/// </summary>
		/// <returns>List of sell conditions for this strategy</returns>
		private List<SellCondition> GetSellConditions()
		{
			// Always have a max time in market and an absolute stop for sell conditions.
			return new List<SellCondition>()
			{
				new StopSellCondition(Simulator.Config.GavalasStopPercent),
				//new StopOneBarTrailingHighLow(),
				new MaxLengthSellCondition(Simulator.Config.GavalasMaxBarsOpen),
			};
		}

		/// <summary>
		/// One last check to filter out bad buys.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <returns>Returns true if the situation passes and we should buy</returns>
		private bool DoesPassFilters(int currentBar)
		{
			//DmiAdx dmiAdx = (DmiAdx)_dependents[3];
			//if (dmiAdx.Adx[currentBar] > 50.0)
			//{
			//	return false;
			//}

			AverageVolume vol = (AverageVolume)_dependents[4];
			if (vol.Avg[currentBar] < 250000)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Adds extra info to the order so that we can analyze it later.
		/// </summary>
		/// <param name="o">Order that was just placed</param>
		/// <param name="currentBar">Current simulation bar</param>
		private void AddExtraOrderInfo(Order o, int currentBar)
		{
			o.AddExtraInfo(() =>
			{
				DtOscillator ind = (DtOscillator)_dependents[1];
				return new KeyValuePair<string, object>("dtosc %k", (object)Math.Round(ind.SK[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				DtOscillator ind = (DtOscillator)_dependents[1];
				return new KeyValuePair<string, object>("dtosc %d", (object)Math.Round(ind.SD[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				GavalasHistogram ind = (GavalasHistogram)_dependents[2];
				return new KeyValuePair<string, object>("histogram", (object)Math.Round(ind.Value[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				DmiAdx ind = (DmiAdx)_dependents[3];
				return new KeyValuePair<string, object>("adx", (object)Math.Round(ind.Adx[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				DmiAdx ind = (DmiAdx)_dependents[3];
				return new KeyValuePair<string, object>("dmi+", (object)Math.Round(ind.DmiPlus[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				DmiAdx ind = (DmiAdx)_dependents[3];
				return new KeyValuePair<string, object>("dmi-", (object)Math.Round(ind.DmiMinus[currentBar], 2));
			});
			
			o.AddExtraInfo(() =>
			{
				AverageVolume ind = (AverageVolume)_dependents[4];
				return new KeyValuePair<string, object>("avgVolume", (object)Math.Round(ind.Avg[currentBar], 2));
			});
		}
	}
}
