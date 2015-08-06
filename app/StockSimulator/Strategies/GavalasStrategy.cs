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
		private double _lastAtrValue = 0.0;
		private double _stopPrice = 0.0;
		private double _expectedGainPrice = 0.0;
		private double _expectedGainPercent = 0.0;
		private double _riskRatio = 0.0;
		private double _stdDev = 0.0;
		private double _lastZoneHitDirection = 0.0;
		private int _barLastZoneHit = 0;

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
				new AverageVolume(tickerData),
				new DmiAdx(tickerData),
				new KeltnerChannel(tickerData),
				new Atr(tickerData)
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
			zones.SetZigZagDeviation(_lastAtrValue * 1.5);
		}

		/// <summary>
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			// Save this so we can use it to set the zigzag deviation.
			_lastAtrValue = ((Atr)_dependents[5]).Value[currentBar];

			if (currentBar < 2)
			{
				return;
			}

			_stdDev = UtilityMethods.StdDev(Data.Close, currentBar, 10);

			GavalasZones zones = (GavalasZones)_dependents[0];
			if (zones.DidBarTouchZone(Data.Low[currentBar], Data.High[currentBar], currentBar) == true)
			{
				_barLastZoneHit = currentBar;
				_lastZoneHitDirection = zones.BuyDirection[currentBar];
			}

			string foundStrategyName = "";
			double buyDirection = 0.0;

			// See if we hit one of our buy zones.
			if (ShouldBuy(currentBar))
			{
				if (ShouldBuyLong(currentBar))
				{
					buyDirection = Order.OrderType.Long;
					foundStrategyName = "BullGavalasStrategy";
				}
				else if (ShouldBuyShort(currentBar))
				{
					buyDirection = Order.OrderType.Short;
					foundStrategyName = "BearGavalasStrategy";
				}
			}

			if (buyDirection != 0.0)
			{
				CalculateTargets(buyDirection, currentBar);

				List<Indicator> dependentIndicators = GetDependentIndicators();

				Order placedOrder = EnterOrder(foundStrategyName, currentBar, buyDirection, Simulator.Config.GavalasMaxRiskAmount,
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

					// Size of order = max amount we'll risk / (current low - stop)
					double lossPerShare = buyDirection > 0 ? Data.Low[currentBar] - _stopPrice : _stopPrice - Data.High[currentBar];
					int sizeOfOrder = Convert.ToInt32(Simulator.Config.GavalasMaxRiskAmount / lossPerShare);

					//if (orderStats.WinPercent >= Simulator.Config.GavalasPercentForBuy && orderStats.Gain > Simulator.Config.GavalasGainForBuy)
					if (sizeOfOrder > 0)
					{
						Bars[currentBar] = new OrderSuggestion(
							orderStats.WinPercent,
							orderStats.Gain,
							foundStrategyName,
							buyDirection,
							sizeOfOrder,
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
				//new OneBarTrailingHighLow(1)
				new MarketBuyCondition()
				//new DirectionBuyCondition(1)
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
				new StopSellCondition(_stopPrice, StopSellCondition.PriceType.Value, false),
				new StopOneBarTrailingChannel((KeltnerChannel)_dependents[4]),
				new StopOscillatorZones((DtOscillator)_dependents[1]),
				new StopNoMovement(5),
				new MaxLengthSellCondition(Simulator.Config.GavalasMaxBarsOpen),
			};
		}

		/// <summary>
		/// One last check to filter out bad buys.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <returns>Returns true if the situation passes and we should buy</returns>
		private bool ShouldBuy(int currentBar)
		{
			AverageVolume vol = (AverageVolume)_dependents[2];
			if (vol.Avg[currentBar] < 250000)
			{
				return false;
			}

			if (currentBar - _barLastZoneHit > 5)
			{
				return false;
			}

			//GavalasZones zones = (GavalasZones)_dependents[0];
			//if (zones.DidBarTouchZone(Data.Low[currentBar], Data.High[currentBar], currentBar) == false)
			//{
			//	return false;
			//}

			//ZigZagWaves.WaveData waveData = zones.GetWaveData(currentBar);
			//double avgWaveHeight = (Math.Abs(waveData.Points[0].Retracement) + Math.Abs(waveData.Points[1].Retracement) + Math.Abs(waveData.Points[2].Retracement)) / 3;
			//if (avgWaveHeight < 4)
			//{
			//	return false;
			//}

			return true;
		}

		/// <summary>
		/// Returns true if we pass all the conditions to buy long.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <returns>See summary</returns>
		private bool ShouldBuyLong(int currentBar)
		{
			GavalasZones zones = (GavalasZones)_dependents[0];

			// Higher timeframe
			if (Data.HigherTimeframeTrend[currentBar] < 0.0)
			{
				return false;
			}

			// Zones buy direction
			if (_lastZoneHitDirection < 0.0)
			{
				return false;
			}

			// Verify with the mechanical buy signal.
			DtOscillator dtosc = (DtOscillator)_dependents[1];
			if (DataSeries.IsBelow(dtosc.SK, DtOscillator.OversoldZone, currentBar, 3) == -1 ||
				DataSeries.CrossAbove(dtosc.SK, dtosc.SD, currentBar, 0) == -1)
			{
				return false;
			}

			// If we are trending make sure the best fits lines are positive.
			DmiAdx dmiAdx = (DmiAdx)_dependents[3];
			if (dmiAdx.Adx[currentBar] >= 25)
			{
				double angle = UtilityMethods.RadianToDegree(Math.Atan(zones.AllBestFitLineSlope[currentBar]));
				if (angle < 0)
				{
					return false;
				}
			}

			// TODO?
			// If we are not trending make sure we are close to the bottom of the channel?

			return true;
		}

		/// <summary>
		/// Returns true if we pass all the conditions to buy short.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <returns>See summary</returns>
		private bool ShouldBuyShort(int currentBar)
		{
			GavalasZones zones = (GavalasZones)_dependents[0];

			// Higher timeframe
			if (Data.HigherTimeframeTrend[currentBar] > 0.0)
			{
				return false;
			}

			// Zones buy direction
			if (_lastZoneHitDirection > 0.0)
			{
				return false;
			}

			// Verify with the mechanical buy signal.
			DtOscillator dtosc = (DtOscillator)_dependents[1];
			if (DataSeries.IsAbove(dtosc.SK, DtOscillator.OverboughtZone, currentBar, 3) == -1 || 
				DataSeries.CrossBelow(dtosc.SK, dtosc.SD, currentBar, 0) == -1)
			{
				return false;
			}

			// If we are trending make sure the best fits lines are negative.
			DmiAdx dmiAdx = (DmiAdx)_dependents[3];
			if (dmiAdx.Adx[currentBar] >= 25)
			{
				double angle = UtilityMethods.RadianToDegree(Math.Atan(zones.AllBestFitLineSlope[currentBar]));
				if (angle > 0)
				{
					return false;
				}
			}

			// TODO?
			// If we are not trending make sure we are close to the top of the channel?

			return true;
		}

		/// <summary>
		/// Calculates the stop and expected profit targets for this soon to be order.
		/// </summary>
		/// <param name="buyDirection">Direction of the order (1 for long, -1 for short)</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		private void CalculateTargets(double buyDirection, int currentBar)
		{
			GavalasZones zones = (GavalasZones)_dependents[0];
			KeltnerChannel kelt = (KeltnerChannel)_dependents[4];

			//double high = Math.Max(zones.HitZone[currentBar].High, Data.High[currentBar]);
			//double low = Math.Min(zones.HitZone[currentBar].Low, Data.Low[currentBar]);

			double high = kelt.Upper[currentBar];
			double low = kelt.Lower[currentBar];

			// Place a protective stop above/below the hit zone using standard deviations
			double stddevStop = _stdDev * 0.5;
			_stopPrice = buyDirection > 0.0 ? low - stddevStop : high + stddevStop;

			// Expected gain is the difference from other side of the Keltner channel to the start of the zone.
			_expectedGainPrice = buyDirection > 0.0 ? kelt.Upper[currentBar] :
				kelt.Lower[currentBar];
			_expectedGainPercent = buyDirection > 0.0 ? UtilityMethods.PercentChange(Data.Close[currentBar], kelt.Upper[currentBar]) :
				UtilityMethods.PercentChange(kelt.Lower[currentBar], Data.Close[currentBar]);

			// Risk reward ratio is amount we expect to gain / stop
			_riskRatio = _expectedGainPercent / (buyDirection > 0.0 ? UtilityMethods.PercentChange(_stopPrice, low) : UtilityMethods.PercentChange(high, _stopPrice));
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
				return new KeyValuePair<string, object>("expectedGain", Math.Round(_expectedGainPercent, 2));
			});

			o.AddExtraInfo(() =>
			{
				return new KeyValuePair<string, object>("riskRatio", Math.Round(_riskRatio, 2));
			});

			////////////////////////////////////////////////////////////////////////////
			// DEBUG INFO
			////////////////////////////////////////////////////////////////////////////

			//o.AddExtraInfo(() =>
			//{
			//	GavalasZones ind = (GavalasZones)_dependents[0];
			//	return new KeyValuePair<string, object>("slopehigh", (object)Math.Round(UtilityMethods.RadianToDegree(Math.Atan(ind.HighBestFitLineSlope[currentBar])), 2));
			//});

			//o.AddExtraInfo(() =>
			//{
			//	GavalasZones ind = (GavalasZones)_dependents[0];
			//	return new KeyValuePair<string, object>("slopelow", (object)Math.Round(UtilityMethods.RadianToDegree(Math.Atan(ind.LowBestFitLineSlope[currentBar])), 2));
			//});

			//o.AddExtraInfo(() =>
			//{
			//	GavalasZones ind = (GavalasZones)_dependents[0];
			//	return new KeyValuePair<string, object>("slopeall", (object)Math.Round(UtilityMethods.RadianToDegree(Math.Atan(ind.AllBestFitLineSlope[currentBar])), 2));
			//});

			//// Num bars of the waves
			//o.AddExtraInfo(() =>
			//{
			//	GavalasZones ind = (GavalasZones)_dependents[0];
			//	ZigZagWaves.WaveData waves = ind.GetWaveData(currentBar);
			//	int barLength = waves.Points[0].Bar - waves.Points[1].Bar;
			//	return new KeyValuePair<string, object>("wave3bars", barLength);
			//});

			//o.AddExtraInfo(() =>
			//{
			//	GavalasZones ind = (GavalasZones)_dependents[0];
			//	ZigZagWaves.WaveData waves = ind.GetWaveData(currentBar);
			//	int barLength = waves.Points[1].Bar - waves.Points[2].Bar;
			//	return new KeyValuePair<string, object>("wave2bars", barLength);
			//});

			//o.AddExtraInfo(() =>
			//{
			//	GavalasZones ind = (GavalasZones)_dependents[0];
			//	ZigZagWaves.WaveData waves = ind.GetWaveData(currentBar);
			//	int barLength = waves.Points[2].Bar - waves.Points[3].Bar;
			//	return new KeyValuePair<string, object>("wave1bars", barLength);
			//});

			//// Wave diffs between the points.
			//o.AddExtraInfo(() =>
			//{
			//	GavalasZones ind = (GavalasZones)_dependents[0];
			//	ZigZagWaves.WaveData waves = ind.GetWaveData(currentBar);
			//	return new KeyValuePair<string, object>("wave3diff", (object)Math.Round(waves.Points[0].Retracement, 2));
			//});

			//o.AddExtraInfo(() =>
			//{
			//	GavalasZones ind = (GavalasZones)_dependents[0];
			//	ZigZagWaves.WaveData waves = ind.GetWaveData(currentBar);
			//	return new KeyValuePair<string, object>("wave2diff", (object)Math.Round(waves.Points[1].Retracement, 2));
			//});

			//o.AddExtraInfo(() =>
			//{
			//	GavalasZones ind = (GavalasZones)_dependents[0];
			//	ZigZagWaves.WaveData waves = ind.GetWaveData(currentBar);
			//	return new KeyValuePair<string, object>("wave1diff", (object)Math.Round(waves.Points[2].Retracement, 2));
			//});

			// Zone info.
			//o.AddExtraInfo(() =>
			//{
			//	GavalasZones ind = (GavalasZones)_dependents[0];
			//	return new KeyValuePair<string, object>("zonewidth", (object)Math.Round(UtilityMethods.PercentChange(ind.HitZone[currentBar].Low, ind.HitZone[currentBar].High), 2));
			//});

			//o.AddExtraInfo(() =>
			//{
			//	GavalasZones ind = (GavalasZones)_dependents[0];
			//	return new KeyValuePair<string, object>("zonepoints", ind.HitZone[currentBar].NumberOfPoints);
			//});

			//o.AddExtraInfo(() =>
			//{
			//	DmiAdx ind = (DmiAdx)_dependents[3];
			//	return new KeyValuePair<string, object>("adx", (object)Math.Round(ind.Adx[currentBar], 2));
			//});

			//o.AddExtraInfo(() =>
			//{
			//	DmiAdx ind = (DmiAdx)_dependents[3];
			//	return new KeyValuePair<string, object>("dmi+", (object)Math.Round(ind.DmiPlus[currentBar], 2));
			//});

			//o.AddExtraInfo(() =>
			//{
			//	DmiAdx ind = (DmiAdx)_dependents[3];
			//	return new KeyValuePair<string, object>("dmi-", (object)Math.Round(ind.DmiMinus[currentBar], 2));
			//});

			//o.AddExtraInfo(() =>
			//{
			//	Atr ind = (Atr)_dependents[5];
			//	return new KeyValuePair<string, object>("atrnorm", (object)Math.Round(ind.ValueNormalized[currentBar], 4));
			//});

			o.AddExtraInfo(() =>
			{
				KeltnerChannel ind = (KeltnerChannel)_dependents[4];
				return new KeyValuePair<string, object>("keltlow", (object)Math.Round(ind.Lower[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				KeltnerChannel ind = (KeltnerChannel)_dependents[4];
				return new KeyValuePair<string, object>("keltmid", (object)Math.Round(ind.Midline[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				KeltnerChannel ind = (KeltnerChannel)_dependents[4];
				return new KeyValuePair<string, object>("keltup", (object)Math.Round(ind.Upper[currentBar], 2));
			});
		}
	}
}
