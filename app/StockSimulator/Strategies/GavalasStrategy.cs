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
		private double lastAtrValue = 0.0;

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
				new AverageVolume(tickerData),
				new Atr(tickerData),
				new Ppo(tickerData)
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
		/// Called before the indicators are run so the strategy can update
		/// any values in the dependent indicators before they start. Things like
		/// configurable periods and such and such.
		/// </summary>
		/// <param name="currentBar">The current bar in the simulation</param>
		protected override void OnBeforeIndicatorRun(int currentBar)
		{
			// Set the zigzag value to be based on the atr of the last frame. This 
			// will hopefully allow the zigzag to adapt to the more recent stock moves
			// and allow it to adapt to different stocks.
			GavalasZones zones = (GavalasZones)_dependents[0];
			zones.SetZigZagDeviation(lastAtrValue * 1.2);
		}

		/// <summary>
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			// Save this so we can use it to set the zigzag deviation.
			lastAtrValue = ((Atr)_dependents[5]).Value[currentBar];

			if (currentBar < 2)
			{
				return;
			}

			GavalasZones zones = (GavalasZones)_dependents[0];

			double buyDirection = zones.BuyDirection[currentBar];
			string foundStrategyName = "";

			// See if we hit one of our buy zones.
			if (ShouldBuy(buyDirection, currentBar))
			{
				if (buyDirection > 0.0 && ShouldBuyLong(currentBar))
				{
					foundStrategyName = "BullGavalasStrategy";
				}
				else if (buyDirection < 0.0 && ShouldBuyShort(currentBar))
				{
					foundStrategyName = "BearGavalasStrategy";
				}
			}

			if (foundStrategyName.Length > 0)
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
				//new OneBarTrailingHighLow(2)
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
				new StopSellCondition(Simulator.Config.GavalasStopPercent, false),
				new ProfitSellCondition(Simulator.Config.GavalasProfitPercent),
				//new StopOneBarTrailingHighLow(false),
				new MaxLengthSellCondition(Simulator.Config.GavalasMaxBarsOpen),
			};
		}

		/// <summary>
		/// One last check to filter out bad buys.
		/// </summary>
		/// <param name="buyDirection">Direction of the order (1 for long -1 for short)</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <returns>Returns true if the situation passes and we should buy</returns>
		private bool ShouldBuy(double buyDirection, int currentBar)
		{
			if (buyDirection == 0.0)
			{
				return false;
			}

			AverageVolume vol = (AverageVolume)_dependents[4];
			if (vol.Avg[currentBar] < 250000)
			{
				return false;
			}

			GavalasZones zones = (GavalasZones)_dependents[0];
			if (zones.DidBarTouchZone(Data.Low[currentBar], Data.High[currentBar], currentBar) == false)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns true if we pass all the conditions to buy long.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <returns>See summary</returns>
		private bool ShouldBuyLong(int currentBar)
		{
			// Higher timeframe
			if (Data.HigherTimeframeTrend[currentBar] < 0.0)
			{
				return false;
			}

			// Verify with the mechanical buy signal.
			DtOscillator dtosc = (DtOscillator)_dependents[1];
			if (dtosc.SD[currentBar] > 25.0 || dtosc.SK[currentBar] > 25.0)
			{
				return false;
			}

			// Are we in a very strong downtrend? If so the upside is limited.
			//DmiAdx dmiAdx = (DmiAdx)_dependents[3];
			//if (dmiAdx.DmiMinus[currentBar] > 35.0 && dmiAdx.DmiPlus[currentBar] < 10.0)
			//{
			//	return false;
			//}

			return true;
		}

		/// <summary>
		/// Returns true if we pass all the conditions to buy short.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <returns>See summary</returns>
		private bool ShouldBuyShort(int currentBar)
		{
			// Higher timeframe
			if (Data.HigherTimeframeTrend[currentBar] > 0.0)
			{
				return false;
			}

			// Verify with the mechanical buy signal.
			DtOscillator dtosc = (DtOscillator)_dependents[1];
			if (dtosc.SD[currentBar] < 75.0 || dtosc.SK[currentBar] < 75.0)
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
				GavalasZones ind = (GavalasZones)_dependents[0];
				return new KeyValuePair<string, object>("slopehigh", (object)Math.Round(ind.HighBestFitLineSlope[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				GavalasZones ind = (GavalasZones)_dependents[0];
				return new KeyValuePair<string, object>("slopelow", (object)Math.Round(ind.LowBestFitLineSlope[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				GavalasZones ind = (GavalasZones)_dependents[0];
				return new KeyValuePair<string, object>("slopeall", (object)Math.Round(ind.AllBestFitLineSlope[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				GavalasZones ind = (GavalasZones)_dependents[0];
				ZigZagWaves.WaveData waves = ind.GetWaveData(currentBar);
				int barLength = waves.Points[0].Bar - waves.Points[1].Bar;
				return new KeyValuePair<string, object>("wave3bars", barLength);
			});

			o.AddExtraInfo(() =>
			{
				GavalasZones ind = (GavalasZones)_dependents[0];
				ZigZagWaves.WaveData waves = ind.GetWaveData(currentBar);
				int barLength = waves.Points[1].Bar - waves.Points[2].Bar;
				return new KeyValuePair<string, object>("wave2bars", barLength);
			});

			o.AddExtraInfo(() =>
			{
				GavalasZones ind = (GavalasZones)_dependents[0];
				ZigZagWaves.WaveData waves = ind.GetWaveData(currentBar);
				int barLength = waves.Points[2].Bar - waves.Points[3].Bar;
				return new KeyValuePair<string, object>("wave1bars", barLength);
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
				Atr ind = (Atr)_dependents[5];
				return new KeyValuePair<string, object>("atrnorm", (object)Math.Round(ind.ValueNormalized[currentBar], 4));
			});

			o.AddExtraInfo(() =>
			{
				Ppo ind = (Ppo)_dependents[6];
				return new KeyValuePair<string, object>("ppo", (object)Math.Round(ind.Value[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				Ppo ind = (Ppo)_dependents[6];
				return new KeyValuePair<string, object>("pposmooth", (object)Math.Round(ind.Smoothed[currentBar], 2));
			});
		}
	}
}
