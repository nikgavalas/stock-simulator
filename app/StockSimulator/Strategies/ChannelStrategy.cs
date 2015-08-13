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
		private double _maxBuyPrice;
		private double _expectedGainPrice;
		private double _expectedGainPercent;
		private double _expectedLossPercent;
		private double _expectedLossPerShare;
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
				new DtOscillator(tickerData)
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
				if (sizeOfOrder > 0)
				{
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
						if (_riskRatio >= Simulator.Config.ChannelMinRiskRatio && _expectedGainPercent >= Simulator.Config.ChannelMinExpectedGain)
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
				//new MarketBuyCondition()
				//new DirectionBuyCondition(1)
				new LimitBuyCondition(_maxBuyPrice, 1)
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

			// Make sure we are trading a stock with the right volitility.
			//double atrNormalized = Data.HigherTimeframeValues["Atr"][currentBar] / Data.HigherTimeframeValues["Close"][currentBar];
			//if (atrNormalized < 0.06 || atrNormalized > 0.10)
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
			// Higher timeframe
			if (Data.HigherTimeframeTrend[currentBar] < 0.0)
			{
				return false;
			}

			KeltnerChannel kelt = (KeltnerChannel)_dependents[2];
			//if (DataSeries.CrossBelow(Data.Low, kelt.Lower, currentBar, 5) == -1)
			//{
			//	return false;
			//}

			if (Math.Max(Data.Open[currentBar], Data.Close[currentBar]) > kelt.Midline[currentBar])
			{
				return false;
			}

			DtOscillator dtosc = (DtOscillator)_dependents[3];
			if (DataSeries.IsBelow(dtosc.SD, DtOscillator.OversoldZone, currentBar, 3) == -1 ||
				DataSeries.IsBelow(dtosc.SK, DtOscillator.OversoldZone, currentBar, 3) == -1 || 
				DataSeries.CrossAbove(dtosc.SK, dtosc.SD, currentBar, 0) == -1)
			{
				return false;
			}

			// Make sure we are not in a heavy trend the other way.
			//DmiAdx dmiAdx = (DmiAdx)_dependents[1];
			//if (dmiAdx.Adx[currentBar] > 30 && dmiAdx.DmiMinus[currentBar] > dmiAdx.DmiPlus[currentBar] && 
			//	UtilityMethods.LineAngle(currentBar - 2, dmiAdx.Adx[currentBar - 2], currentBar, dmiAdx.Adx[currentBar]) > 5)
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

			KeltnerChannel kelt = (KeltnerChannel)_dependents[2];
			//if (DataSeries.CrossAbove(Data.High, kelt.Upper, currentBar, 5) == -1)
			//{
			//	return false;
			//}

			if (Math.Min(Data.Open[currentBar], Data.Close[currentBar]) < kelt.Midline[currentBar])
			{
				return false;
			}

			DtOscillator dtosc = (DtOscillator)_dependents[3];
			if (DataSeries.IsAbove(dtosc.SD, DtOscillator.OverboughtZone, currentBar, 3) == -1 ||
				DataSeries.IsAbove(dtosc.SK, DtOscillator.OverboughtZone, currentBar, 3) == -1 ||
				DataSeries.CrossBelow(dtosc.SK, dtosc.SD, currentBar, 0) == -1)
			{
				return false;
			}

			// Make sure we are not in a heavy trend the other way.
			//DmiAdx dmiAdx = (DmiAdx)_dependents[1];
			//if (dmiAdx.Adx[currentBar] > 30 && dmiAdx.DmiPlus[currentBar] > dmiAdx.DmiMinus[currentBar] &&
			//	UtilityMethods.LineAngle(currentBar - 2, dmiAdx.Adx[currentBar - 2], currentBar, dmiAdx.Adx[currentBar]) > 5)
			//{
			//	return false;
			//}

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

			_maxBuyPrice = kelt.Midline[currentBar];

			if (buyDirection > 0.0)
			{
				_stopPrice = Data.Low[currentBar] - Data.TickSize;
				_expectedGainPrice = Data.HigherTimeframeValues["KeltnerUpper"][currentBar];
				_expectedGainPercent = UtilityMethods.PercentChange(Data.Close[currentBar], _expectedGainPrice);
				_expectedLossPercent = UtilityMethods.PercentChange(_stopPrice, Data.Close[currentBar]);
				_expectedLossPerShare = Data.Close[currentBar] - _stopPrice;
				_riskRatio = _expectedGainPercent / _expectedLossPercent;
			}
			else
			{
				// TODO fix these.
				//low = kelt.Upper[currentBar];// +_stdDev;
				//_stopPrice = low;
				//_expectedGainPrice = Data.HigherTimeframeValues["KeltnerLower"][currentBar];
				//_expectedGainPercent = UtilityMethods.PercentChange(_expectedGainPrice, Data.Close[currentBar]);
				//_riskRatio = _stopPrice <= Data.High[currentBar] ? 2.0 :
				//	_expectedGainPercent / UtilityMethods.PercentChange(Data.High[currentBar], _stopPrice);
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
			int sizeOfOrder;

			// If the loss per share is really low we can overflow our value. If thats the case then
			// essentially it's saying there is very little risk and a lot of potential gain. So we'll
			// just use the max order size.
			try
			{
				sizeOfOrder = Convert.ToInt32(Data.Close[currentBar] * (Simulator.Config.GavalasMaxRiskAmount / _expectedLossPerShare));
				if (sizeOfOrder > Simulator.Config.ChannelMaxOrderSize)
				{
					sizeOfOrder = Simulator.Config.ChannelMaxOrderSize;
				}
			}
			catch (Exception)
			{
				sizeOfOrder = Simulator.Config.ChannelMaxOrderSize;
			}

			return sizeOfOrder;
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

			o.AddExtraInfo(() =>
			{
				return new KeyValuePair<string, object>("highatrnorm", (object)Math.Round(Data.HigherTimeframeValues["Atr"][currentBar] / Data.HigherTimeframeValues["Close"][currentBar], 4));
			});

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

			//o.AddExtraInfo(() =>
			//{
			//	KeltnerChannel ind = (KeltnerChannel)_dependents[2];
			//	return new KeyValuePair<string, object>("keltlow", (object)Math.Round(ind.Lower[currentBar], 2));
			//});

			//o.AddExtraInfo(() =>
			//{
			//	KeltnerChannel ind = (KeltnerChannel)_dependents[2];
			//	return new KeyValuePair<string, object>("keltmid", (object)Math.Round(ind.Midline[currentBar], 2));
			//});

			//o.AddExtraInfo(() =>
			//{
			//	KeltnerChannel ind = (KeltnerChannel)_dependents[2];
			//	return new KeyValuePair<string, object>("keltup", (object)Math.Round(ind.Upper[currentBar], 2));
			//});
		}
	}
}
