using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Indicators;
using StockSimulator.Strategies;
using System.Diagnostics;

namespace StockSimulator.Core
{
	/// <summary>
	/// Knows how to create other runables with and instrument and config data
	/// </summary>
	public class RunnableFactory
	{
		private Dictionary<int, Runnable> _createdItems;
		private TickerData _tickerData;

		/// <summary>
		/// Constructor
		/// </summary>
		public RunnableFactory(TickerData tickerData)
		{
			_tickerData = tickerData;
			_createdItems = new Dictionary<int, Runnable>();
		}

		/// <summary>
		/// If the runnable has already been created, returns that object. If not then
		/// returns an new runnable object based on the name and the instrument.
		/// </summary>
		/// <param name="runnableName">Name of the runnable</param>
		/// <returns>The runnable object</returns>
		public Runnable GetRunnable(string runnableName)
		{
			Runnable requestedItem = null;

			// See if the runnable is created already and return that object if it is.
			int key = runnableName.GetHashCode();
			if (_createdItems.ContainsKey(key))
			{
				requestedItem = _createdItems[key];
			}
			else
			{
				switch (runnableName)
				{
					// Indicators.
					case "Bollinger":
						requestedItem = new Bollinger(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "BullBeltHold":
						requestedItem = new BullBeltHold(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "BullEngulfing":
						requestedItem = new BullEngulfing(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "BullHarami":
						requestedItem = new BullHarami(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "BullHaramiCross":
						requestedItem = new BullHaramiCross(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Cci14":
						requestedItem = new Cci(_tickerData, this, 14);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Doji":
						requestedItem = new Doji(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Hammer":
						requestedItem = new Hammer(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "KeltnerChannel":
						requestedItem = new KeltnerChannel(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Macd":
						requestedItem = new Macd(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Momentum14":
						requestedItem = new Momentum(_tickerData, this, 14);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "MorningStar":
						requestedItem = new MorningStar(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "PiercingLine":
						requestedItem = new PiercingLine(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "RisingThreeMethods":
						requestedItem = new RisingThreeMethods(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Rsi14":
						requestedItem = new Rsi(_tickerData, this, 14);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Sma":
						requestedItem = new Sma(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "StickSandwitch":
						requestedItem = new StickSandwitch(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "StochasticsFast":
						requestedItem = new StochasticsFast(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Stochastics":
						requestedItem = new Stochastics(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Swing":
						requestedItem = new Swing(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Trend":
						requestedItem = new Trend(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "ThreeWhiteSoldiers":
						requestedItem = new ThreeWhiteSoldiers(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Trix":
						requestedItem = new Trix(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "UpsideTasukiGap":
						requestedItem = new UpsideTasukiGap(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "WilliamsR":
						requestedItem = new WilliamsR(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					///////////////////////////// Strategies ////////////////////////////

					//
					// Bull
					//

					case "BestOfSubStrategies":
						requestedItem = new BestOfSubStrategies(_tickerData, this);
						break;

					case "BullBollingerExtended":
						requestedItem = new BullBollingerExtended(_tickerData, this);
						break;

					case "BullBeltHoldFound":
						requestedItem = new BullBeltHoldFound(_tickerData, this);
						break;

					case "BullEngulfingFound":
						requestedItem = new BullEngulfingFound(_tickerData, this);
						break;

					case "BullHaramiFound":
						requestedItem = new BullHaramiFound(_tickerData, this);
						break;

					case "BullHaramiCrossFound":
						requestedItem = new BullHaramiCrossFound(_tickerData, this);
						break;

					case "BullCciCrossover":
						requestedItem = new BullCciCrossover(_tickerData, this);
						break;

					case "DojiFound":
						requestedItem = new DojiFound(_tickerData, this);
						break;

					case "HammerFound":
						requestedItem = new HammerFound(_tickerData, this);
						break;

					case "BullKeltnerCloseAbove":
						requestedItem = new BullKeltnerCloseAbove(_tickerData, this);
						break;

					case "BullMacdCrossover":
						requestedItem = new BullMacdCrossover(_tickerData, this);
						break;

					case "BullMomentumCrossover":
						requestedItem = new BullMomentumCrossover(_tickerData, this);
						break;

					case "MorningStarFound":
						requestedItem = new MorningStarFound(_tickerData, this);
						break;

					case "PiercingLineFound":
						requestedItem = new PiercingLineFound(_tickerData, this);
						break;

					case "RisingThreeMethodsFound":
						requestedItem = new RisingThreeMethodsFound(_tickerData, this);
						break;

					case "BullRsiCrossover30":
						requestedItem = new BullRsiCrossover30(_tickerData, this);
						break;

					case "BullSmaCrossover":
						requestedItem = new BullSmaCrossover(_tickerData, this);
						break;

					case "StickSandwitchFound":
						requestedItem = new StickSandwitchFound(_tickerData, this);
						break;

					case "BullStochasticsFastCrossover":
						requestedItem = new BullStochasticsFastCrossover(_tickerData, this);
						break;

					case "BullStochasticsCrossover":
						requestedItem = new BullStochasticsCrossover(_tickerData, this);
						break;

					case "BullSwingStart":
						requestedItem = new BullSwingStart(_tickerData, this);
						break;

					case "ThreeWhiteSoldiersFound":
						requestedItem = new ThreeWhiteSoldiersFound(_tickerData, this);
						break;

					case "BullTrendStart":
						requestedItem = new BullTrendStart(_tickerData, this);
						break;

					case "BullTrixSignalCrossover":
						requestedItem = new BullTrixSignalCrossover(_tickerData, this);
						break;

					case "BullTrixZeroCrossover":
						requestedItem = new BullTrixZeroCrossover(_tickerData, this);
						break;

					case "UpsideTasukiGapFound":
						requestedItem = new UpsideTasukiGapFound(_tickerData, this);
						break;

					case "BullWilliamsRCrossover":
						requestedItem = new BullWilliamsRCrossover(_tickerData, this);
						break;

					//
					// Bear
					//

					case "BearBollingerExtended":
						requestedItem = new BearBollingerExtended(_tickerData, this);
						break;

					case "BearCciCrossover":
						requestedItem = new BearCciCrossover(_tickerData, this);
						break;

					case "BearKeltnerCloseAbove":
						requestedItem = new BearKeltnerCloseAbove(_tickerData, this);
						break;

					case "BearMacdCrossover":
						requestedItem = new BearMacdCrossover(_tickerData, this);
						break;

					case "BearMomentumCrossover":
						requestedItem = new BearMomentumCrossover(_tickerData, this);
						break;

					case "BearRsiCrossover70":
						requestedItem = new BearRsiCrossover70(_tickerData, this);
						break;

					case "BearSmaCrossover":
						requestedItem = new BearSmaCrossover(_tickerData, this);
						break;

					case "BearStochasticsCrossover":
						requestedItem = new BearStochasticsCrossover(_tickerData, this);
						break;

					case "BearStochasticsFastCrossover":
						requestedItem = new BearStochasticsFastCrossover(_tickerData, this);
						break;

					case "BearSwingStart":
						requestedItem = new BearSwingStart(_tickerData, this);
						break;

					case "BearTrendStart":
						requestedItem = new BearTrendStart(_tickerData, this);
						break;

					case "BearTrixSignalCrossover":
						requestedItem = new BearTrixSignalCrossover(_tickerData, this);
						break;

					case "BearTrixZeroCrossover":
						requestedItem = new BearTrixZeroCrossover(_tickerData, this);
						break;

					case "BearWilliamsRCrossover":
						requestedItem = new BearWilliamsRCrossover(_tickerData, this);
						break;

					default:
						throw new Exception("Trying to create a runnable that doesn't exist");
				}

				_createdItems[key] = requestedItem;
			}

			return requestedItem;
		}
	}
}
