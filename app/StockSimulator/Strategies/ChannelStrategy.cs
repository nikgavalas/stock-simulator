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
	public class ChannelStrategy : RootSubStrategy
	{
		private double _stdDev;
		private double _stopPrice;
		private double _expectedGainPrice;
		private double _expectedGainPercent;
		private double _riskRatio;

		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		public ChannelStrategy(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				new AverageVolume(tickerData),
				new DmiAdx(tickerData),
				new KeltnerChannel(tickerData),
				new Rsi3m3(tickerData),
				new BressertDss(tickerData)
			};
		}

		/// <summary>
		/// Returns the name of this strategy.
		/// </summary>
		/// <returns>The name of this strategy</returns>
		public override string ToString()
		{
			return "ChannelStrategy";
		}

		/// <summary>
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			// Give the indicators some time to warm up.
			if (currentBar < 50)
			{
				return;
			}

			_stdDev = UtilityMethods.StdDev(Data.Close, currentBar, 10);

			string foundStrategyName = "";
			double buyDirection = 0.0;

			if (ShouldBuy(currentBar))
			{
				if (ShouldBuyLong(currentBar))
				{
					buyDirection = Order.OrderType.Long;
					foundStrategyName = "BullChannelStrategy";
				}
				//else if (ShouldBuyShort(currentBar))
				//{
				//	buyDirection = Order.OrderType.Short;
				//	foundStrategyName = "BearChannelStrategy";
				//}
			}

			if (buyDirection != 0.0)
			{
				CalculateTargets(buyDirection, currentBar);

				int sizeOfOrder = CalculateOrderSize(buyDirection, currentBar);

				List<Indicator> dependentIndicators = GetDependentIndicators();

				Order placedOrder = EnterOrder(foundStrategyName, currentBar, buyDirection, sizeOfOrder,
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

					//if (orderStats.WinPercent >= Simulator.Config.GavalasPercentForBuy && orderStats.Gain > Simulator.Config.GavalasGainForBuy)
					if (sizeOfOrder > 0 && _riskRatio > Simulator.Config.ChannelMinRiskRatio)
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
		/// Returns the size of the order in dollars based on our risk factors.
		/// </summary>
		/// <param name="buyDirection">Direction of the order (long or short)</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <returns>See summary</returns>
		private int CalculateOrderSize(double buyDirection, int currentBar)
		{
			// Size of order = max amount we'll risk / (current low - stop)
			double lossPerShare = buyDirection > 0.0 ? Data.Low[currentBar] - _stopPrice : _stopPrice - Data.High[currentBar];
			int sizeOfOrder = Convert.ToInt32(Data.Close[currentBar] * (Simulator.Config.GavalasMaxRiskAmount / lossPerShare));
			if (sizeOfOrder > Simulator.Config.ChannelMaxOrderSize)
			{
				sizeOfOrder = Simulator.Config.ChannelMaxOrderSize;
			}

			return sizeOfOrder;
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
				new StopOneBarTrailingChannel((KeltnerChannel)_dependents[2]),
				//new StopOscillatorZones((DtOscillator)_dependents[1]),
				new StopNoMovement(5),
				new MaxLengthSellCondition(Simulator.Config.ChannelMaxBarsOpen),
			};
		}

		/// <summary>
		/// One last check to filter out bad buys.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <returns>Returns true if the situation passes and we should buy</returns>
		private bool ShouldBuy(int currentBar)
		{
			AverageVolume vol = (AverageVolume)_dependents[0];
			if (vol.Avg[currentBar] < 250000)
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

			KeltnerChannel kelt = (KeltnerChannel)_dependents[2];
			if (DataSeries.CrossBelow(Data.Low, kelt.Lower, currentBar, 5) == -1)
			{
				return false;
			}

			if (Math.Max(Data.Open[currentBar], Data.Close[currentBar]) > kelt.Midline[currentBar])
			{
				return false;
			}

			Rsi3m3 rsi = (Rsi3m3)_dependents[3];
			if (DataSeries.IsValley(rsi.Value, currentBar, 0) == -1)
			{
				return false;
			}

			//BressertDss dss = (BressertDss)_dependents[4];
			//if (DataSeries.IsBelow(dss.Value, 50.0, currentBar, 2) == -1 || DataSeries.IsValley(dss.Value, currentBar, 0) == -1)
			//{
			//	return false;
			//}

			// Make sure we are not in a heavy trend the other way.
			DmiAdx dmiAdx = (DmiAdx)_dependents[1];
			if (dmiAdx.Adx[currentBar] > 30 && dmiAdx.DmiMinus[currentBar] > dmiAdx.DmiPlus[currentBar] && 
				UtilityMethods.LineAngle(currentBar - 2, dmiAdx.Adx[currentBar - 2], currentBar, dmiAdx.Adx[currentBar]) > 5)
			{
				return false;
			}

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

			Rsi3m3 rsi = (Rsi3m3)_dependents[3];
			if (DataSeries.IsAbove(rsi.Value, 70, currentBar, 2) == -1 || UtilityMethods.IsPeak(rsi.Value, currentBar) == false)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Calculates the stop and expected profit targets for this soon to be order.
		/// </summary>
		/// <param name="buyDirection">Direction of the order (1 for long, -1 for short)</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		private void CalculateTargets(double buyDirection, int currentBar)
		{
			KeltnerChannel kelt = (KeltnerChannel)_dependents[2];
			double low;

			if (buyDirection > 0.0)
			{
				low = kelt.Lower[currentBar] - _stdDev;
				//low = Math.Min(low, Data.Low[currentBar] - _stdDev);
				_stopPrice = low;
				_expectedGainPrice = Data.HigherTimeframeValues["KeltnerUpper"][currentBar];
				_expectedGainPercent = UtilityMethods.PercentChange(Data.Close[currentBar], _expectedGainPrice);
				_riskRatio = _expectedGainPercent / UtilityMethods.PercentChange(_stopPrice, Data.Low[currentBar]);
			}
			else
			{
				low = UtilityMethods.Min(Data.HigherTimeframeValues["Close"], currentBar, 5);
				low = Math.Min(low, Data.Low[currentBar]);
				_stopPrice = low;
				_expectedGainPrice = Data.HigherTimeframeValues["KeltnerUpper"][currentBar];
				_expectedGainPercent = UtilityMethods.PercentChange(Data.Close[currentBar], _expectedGainPrice);
				_riskRatio = _expectedGainPercent / UtilityMethods.PercentChange(_stopPrice, Data.Close[currentBar]);
			}
			//double high = Data.HigherTimeframeValues["KeltnerUpper"][currentBar];

			//// Place a protective stop above/below the hit zone using standard deviations
			//double stddevStop = _stdDev * 0.5;
			//_stopPrice = buyDirection > 0.0 ? low - stddevStop : high + stddevStop;

			//// Expected gain is the difference from other side of the Keltner channel to the start of the zone.
			//_expectedGainPrice = buyDirection > 0.0 ? kelt.Upper[currentBar] :
			//	kelt.Lower[currentBar];
			//_expectedGainPercent = buyDirection > 0.0 ? UtilityMethods.PercentChange(Data.Close[currentBar], kelt.Upper[currentBar]) :
			//	UtilityMethods.PercentChange(kelt.Lower[currentBar], Data.Close[currentBar]);

			// Risk reward ratio is amount we expect to gain / stop
			//_riskRatio = _expectedGainPercent / (buyDirection > 0.0 ? UtilityMethods.PercentChange(_stopPrice, low) : UtilityMethods.PercentChange(high, _stopPrice));
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
				return new KeyValuePair<string, object>("expectedGainPrice", Math.Round(_expectedGainPrice, 2));
			});

			o.AddExtraInfo(() =>
			{
				return new KeyValuePair<string, object>("riskRatio", Math.Round(_riskRatio, 2));
			});

			o.AddExtraInfo(() =>
			{
				return new KeyValuePair<string, object>("stopPrice", Math.Round(_stopPrice, 2));
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
				KeltnerChannel ind = (KeltnerChannel)_dependents[2];
				return new KeyValuePair<string, object>("keltlow", (object)Math.Round(ind.Lower[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				KeltnerChannel ind = (KeltnerChannel)_dependents[2];
				return new KeyValuePair<string, object>("keltmid", (object)Math.Round(ind.Midline[currentBar], 2));
			});

			o.AddExtraInfo(() =>
			{
				KeltnerChannel ind = (KeltnerChannel)_dependents[2];
				return new KeyValuePair<string, object>("keltup", (object)Math.Round(ind.Upper[currentBar], 2));
			});
		}
	}
}
