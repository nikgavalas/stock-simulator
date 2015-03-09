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
						break;

					case "BullBeltHold":
						requestedItem = new BullBeltHold(_tickerData, this);
						break;

					case "BullEngulfing":
						requestedItem = new BullEngulfing(_tickerData, this);
						break;

					case "BullHarami":
						requestedItem = new BullHarami(_tickerData, this);
						break;

					case "BullHaramiCross":
						requestedItem = new BullHaramiCross(_tickerData, this);
						break;

					case "Cci14":
						requestedItem = new Cci(_tickerData, this, 14);
						break;

					case "Doji":
						requestedItem = new Doji(_tickerData, this);
						break;

					case "Hammer":
						requestedItem = new Hammer(_tickerData, this);
						break;

					case "KeltnerChannel":
						requestedItem = new KeltnerChannel(_tickerData, this);
						break;

					case "Macd":
						requestedItem = new Macd(_tickerData, this);
						break;

					case "Momentum14":
						requestedItem = new Momentum(_tickerData, this, 14);
						break;

					case "MorningStar":
						requestedItem = new MorningStar(_tickerData, this);
						break;

					case "PiercingLine":
						requestedItem = new PiercingLine(_tickerData, this);
						break;

					case "RisingThreeMethods":
						requestedItem = new RisingThreeMethods(_tickerData, this);
						break;

					case "Rsi14":
						requestedItem = new Rsi(_tickerData, this, 14);
						break;

					case "Sma":
						requestedItem = new Sma(_tickerData, this);
						break;

					case "StickSandwitch":
						requestedItem = new StickSandwitch(_tickerData, this);
						break;

					case "StochasticsFast":
						requestedItem = new StochasticsFast(_tickerData, this);
						break;

					case "Swing":
						requestedItem = new Swing(_tickerData, this);
						break;

					case "Trend":
						requestedItem = new Trend(_tickerData, this);
						break;

					case "ThreeWhiteSoldiers":
						requestedItem = new ThreeWhiteSoldiers(_tickerData, this);
						break;

					case "Trix":
						requestedItem = new Trix(_tickerData, this);
						break;

					case "UpsideTasukiGap":
						requestedItem = new UpsideTasukiGap(_tickerData, this);
						break;

					case "WilliamsR":
						requestedItem = new WilliamsR(_tickerData, this);
						break;

					// Strategies.
					case "BestOfSubStrategies":
						requestedItem = new BestOfSubStrategies(_tickerData, this);
						break;

					case "BollingerExtended":
						requestedItem = new BollingerExtended(_tickerData, this);
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

					case "CciCrossover":
						requestedItem = new CciCrossover(_tickerData, this);
						break;

					case "DojiFound":
						requestedItem = new DojiFound(_tickerData, this);
						break;

					case "HammerFound":
						requestedItem = new HammerFound(_tickerData, this);
						break;

					case "KeltnerCloseAbove":
						requestedItem = new KeltnerCloseAbove(_tickerData, this);
						break;

					case "MacdCrossover":
						requestedItem = new MacdCrossover(_tickerData, this);
						break;

					case "MomentumCrossover":
						requestedItem = new MomentumCrossover(_tickerData, this);
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

					case "RsiCrossover30":
						requestedItem = new RsiCrossover30(_tickerData, this);
						break;

					case "SmaCrossover":
						requestedItem = new SmaCrossover(_tickerData, this);
						break;

					case "StickSandwitchFound":
						requestedItem = new StickSandwitchFound(_tickerData, this);
						break;

					case "StochasticsFastCrossover":
						requestedItem = new StochasticsFastCrossover(_tickerData, this);
						break;

					case "SwingStart":
						requestedItem = new SwingStart(_tickerData, this);
						break;

					case "ThreeWhiteSoldiersFound":
						requestedItem = new ThreeWhiteSoldiersFound(_tickerData, this);
						break;

					case "TrendStart":
						requestedItem = new TrendStart(_tickerData, this);
						break;

					case "TrixSignalCrossover":
						requestedItem = new TrixSignalCrossover(_tickerData, this);
						break;

					case "TrixZeroCrossover":
						requestedItem = new TrixZeroCrossover(_tickerData, this);
						break;

					case "UpsideTasukiGapFound":
						requestedItem = new UpsideTasukiGapFound(_tickerData, this);
						break;

					case "WilliamsRCrossover":
						requestedItem = new WilliamsRCrossover(_tickerData, this);
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
