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
		/// <param name="nameAndParameters">Name of the runnable</param>
		/// <returns>The runnable object</returns>
		public Runnable GetRunnable(string nameAndParameters)
		{
			Runnable requestedItem = null;

			// The name can have parameters to pass to the runnable construtor
			// and are separated by commas.
			// Ex: Rsi,11,3 would create the Rsi and pass the numbers to it in a
			// list. Its up to the indicator to do what it will with each number.
			string[] splitParams = nameAndParameters.Split(',');
			string runnableName = splitParams[0];
			string[] runnableParams = splitParams.Skip(1).Take(splitParams.Length - 1).ToArray();

			// See if the runnable is created already and return that object if it is.
			int key = nameAndParameters.GetHashCode();
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

					case "BressertDss":
						requestedItem = new BressertDss(_tickerData, this, runnableParams);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "BressertTimingBands":
						requestedItem = new BressertTimingBands(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "BearBeltHold":
						requestedItem = new BearBeltHold(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "BearEngulfing":
						requestedItem = new BearEngulfing(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "BearHarami":
						requestedItem = new BearHarami(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "BearHaramiCross":
						requestedItem = new BearHaramiCross(_tickerData, this);
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

					case "DarkCloudCover":
						requestedItem = new DarkCloudCover(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Doji":
						requestedItem = new Doji(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "BearDoji":
						requestedItem = new BearDoji(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "DownsideTasukiGap":
						requestedItem = new DownsideTasukiGap(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "EaseOfMovement":
						requestedItem = new EaseOfMovement(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "EveningStar":
						requestedItem = new EveningStar(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "FallingThreeMethods":
						requestedItem = new FallingThreeMethods(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Hammer":
						requestedItem = new Hammer(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "HangingMan":
						requestedItem = new HangingMan(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "InvertedHammer":
						requestedItem = new InvertedHammer(_tickerData, this);
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

					case "PriceOscillator":
						requestedItem = new PriceOscillator(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "RisingThreeMethods":
						requestedItem = new RisingThreeMethods(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Rsi":
						requestedItem = new Rsi(_tickerData, this, runnableParams);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Rsi3m3":
						requestedItem = new Rsi3m3(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "ShootingStar":
						requestedItem = new ShootingStar(_tickerData, this);
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

					case "StochRsi":
						requestedItem = new StochRsi(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "Trend":
						requestedItem = new Trend(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "ThreeBlackCrows":
						requestedItem = new ThreeBlackCrows(_tickerData, this);
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

					case "UpsideGapTwoCrows":
						requestedItem = new UpsideGapTwoCrows(_tickerData, this);
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

					case "Dmi":
						requestedItem = new Dmi(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "DtOscillator":
						requestedItem = new DtOscillator(_tickerData, this, runnableParams);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "FibonacciZones":
						requestedItem = new FibonacciZones(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "ElliotWaves":
						requestedItem = new ElliotWaves(_tickerData, this);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					case "ZigZag":
						requestedItem = new ZigZag(_tickerData, this, runnableParams);
						Simulator.DataOutput.SaveIndicator((Indicator)requestedItem);
						break;

					///////////////////////////// Strategies ////////////////////////////

					case "BestOfRootStrategies":
						requestedItem = new BestOfRootStrategies(_tickerData, this);
						break;

					case "ComboStrategy":
						requestedItem = new ComboStrategy(_tickerData, this);
						break;

					case "BressertApproach":
						requestedItem = new BressertApproach(_tickerData, this);
						break;

					case "BressertComboStrategy":
						requestedItem = new BressertComboStrategy(_tickerData, this);
						break;

					case "FibonacciRsi3m3":
						requestedItem = new FibonacciRsi3m3(_tickerData, this);
						break;

					case "FibonacciDtOscillator":
						requestedItem = new FibonacciDtOscillator(_tickerData, this);
						break;

					case "ElliotWavesStrategy":
						requestedItem = new ElliotWavesStrategy(_tickerData, this);
						break;

					//
					// Bull
					//

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

					case "BullEaseOfMovement":
						requestedItem = new BullEaseOfMovement(_tickerData, this);
						break;

					case "BullDojiFound":
						requestedItem = new BullDojiFound(_tickerData, this);
						break;

					case "HammerFound":
						requestedItem = new HammerFound(_tickerData, this);
						break;

					case "BullKeltnerExtended":
						requestedItem = new BullKeltnerExtended(_tickerData, this);
						break;

					case "BullMacdCrossover":
						requestedItem = new BullMacdCrossover(_tickerData, this);
						break;

					case "BullMacdMomentum":
						requestedItem = new BullMacdMomentum(_tickerData, this);
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

					case "BullRsiCrossover":
						requestedItem = new BullRsiCrossover(_tickerData, this);
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

					case "BullStochRsiCrossover":
						requestedItem = new BullStochRsiCrossover(_tickerData, this);
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

					case "BullPriceOscillator":
						requestedItem = new BullPriceOscillator(_tickerData, this);
						break;

					case "BullDmi":
						requestedItem = new BullDmi(_tickerData, this);
						break;

					case "BullBressertDss":
						requestedItem = new BullBressertDss(_tickerData, this, runnableParams);
						break;

					case "BullRsi3m3":
						requestedItem = new BullRsi3m3(_tickerData, this);
						break;

					case "BullDtOscillator":
						requestedItem = new BullDtOscillator(_tickerData, this);
						break;

					//////////// Predicted bull strategies ///////////

					case "BullCciCrossoverPredicted":
						requestedItem = new BullCciCrossoverPredicted(_tickerData, this);
						break;

					case "BullDmiPredicted":
						requestedItem = new BullDmiPredicted(_tickerData, this);
						break;

					case "BullEaseOfMovementPredicted":
						requestedItem = new BullEaseOfMovementPredicted(_tickerData, this);
						break;

					case "BullKeltnerExtendedPredicted":
						requestedItem = new BullKeltnerExtendedPredicted(_tickerData, this);
						break;

					case "BullMacdCrossoverPredicted":
						requestedItem = new BullMacdCrossoverPredicted(_tickerData, this);
						break;

					case "BullMomentumCrossoverPredicted":
						requestedItem = new BullMomentumCrossoverPredicted(_tickerData, this);
						break;

					case "BullPriceOscillatorPredicted":
						requestedItem = new BullPriceOscillatorPredicted(_tickerData, this);
						break;

					case "BullRsiCrossoverPredicted":
						requestedItem = new BullRsiCrossoverPredicted(_tickerData, this);
						break;

					case "BullSmaCrossoverPredicted":
						requestedItem = new BullSmaCrossoverPredicted(_tickerData, this);
						break;

					case "BullStochasticsCrossoverPredicted":
						requestedItem = new BullStochasticsCrossoverPredicted(_tickerData, this);
						break;

					case "BullStochasticsFastCrossoverPredicted":
						requestedItem = new BullStochasticsFastCrossoverPredicted(_tickerData, this);
						break;

					case "BullStochRsiCrossoverPredicted":
						requestedItem = new BullStochRsiCrossoverPredicted(_tickerData, this);
						break;

					case "BullTrixSignalCrossoverPredicted":
						requestedItem = new BullTrixSignalCrossoverPredicted(_tickerData, this);
						break;

					case "BullTrixZeroCrossoverPredicted":
						requestedItem = new BullTrixZeroCrossoverPredicted(_tickerData, this);
						break;

					case "BullWilliamsRCrossoverPredicted":
						requestedItem = new BullWilliamsRCrossoverPredicted(_tickerData, this);
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

					case "BearEaseOfMovement":
						requestedItem = new BearEaseOfMovement(_tickerData, this);
						break;

					case "BearDojiFound":
						requestedItem = new BearDojiFound(_tickerData, this);
						break;

					case "BearKeltnerExtended":
						requestedItem = new BearKeltnerExtended(_tickerData, this);
						break;

					case "BearMacdMomentum":
						requestedItem = new BearMacdMomentum(_tickerData, this);
						break;

					case "BearMacdCrossover":
						requestedItem = new BearMacdCrossover(_tickerData, this);
						break;

					case "BearMomentumCrossover":
						requestedItem = new BearMomentumCrossover(_tickerData, this);
						break;

					case "BearRsiCrossover":
						requestedItem = new BearRsiCrossover(_tickerData, this);
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

					case "BearStochRsiCrossover":
						requestedItem = new BearStochRsiCrossover(_tickerData, this);
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

					case "BearBeltHoldFound":
						requestedItem = new BearBeltHoldFound(_tickerData, this);
						break;

					case "BearEngulfingFound":
						requestedItem = new BearEngulfingFound(_tickerData, this);
						break;

					case "BearHaramiFound":
						requestedItem = new BearHaramiFound(_tickerData, this);
						break;

					case "BearHaramiCrossFound":
						requestedItem = new BearHaramiCrossFound(_tickerData, this);
						break;

					case "DarkCloudCoverFound":
						requestedItem = new DarkCloudCoverFound(_tickerData, this);
						break;

					case "DownsideTasukiGapFound":
						requestedItem = new DownsideTasukiGapFound(_tickerData, this);
						break;

					case "EveningStarFound":
						requestedItem = new EveningStarFound(_tickerData, this);
						break;

					case "FallingThreeMethodsFound":
						requestedItem = new FallingThreeMethodsFound(_tickerData, this);
						break;

					case "HangingManFound":
						requestedItem = new HangingManFound(_tickerData, this);
						break;

					case "InvertedHammerFound":
						requestedItem = new InvertedHammerFound(_tickerData, this);
						break;

					case "ShootingStarFound":
						requestedItem = new ShootingStarFound(_tickerData, this);
						break;

					case "ThreeBlackCrowsFound":
						requestedItem = new ThreeBlackCrowsFound(_tickerData, this);
						break;

					case "UpsideGapTwoCrowsFound":
						requestedItem = new UpsideGapTwoCrowsFound(_tickerData, this);
						break;

					case "BearPriceOscillator":
						requestedItem = new BearPriceOscillator(_tickerData, this);
						break;

					case "BearDmi":
						requestedItem = new BearDmi(_tickerData, this);
						break;

					case "BearBressertDss":
						requestedItem = new BearBressertDss(_tickerData, this, runnableParams);
						break;

					case "BearRsi3m3":
						requestedItem = new BearRsi3m3(_tickerData, this);
						break;

					case "BearDtOscillator":
						requestedItem = new BearDtOscillator(_tickerData, this);
						break;

					//////////// Predicted bear strategies ///////////

					case "BearCciCrossoverPredicted":
						requestedItem = new BearCciCrossoverPredicted(_tickerData, this);
						break;

					case "BearDmiPredicted":
						requestedItem = new BearDmiPredicted(_tickerData, this);
						break;

					case "BearEaseOfMovementPredicted":
						requestedItem = new BearEaseOfMovementPredicted(_tickerData, this);
						break;

					case "BearKeltnerExtendedPredicted":
						requestedItem = new BearKeltnerExtendedPredicted(_tickerData, this);
						break;

					case "BearMacdCrossoverPredicted":
						requestedItem = new BearMacdCrossoverPredicted(_tickerData, this);
						break;

					case "BearMomentumCrossoverPredicted":
						requestedItem = new BearMomentumCrossoverPredicted(_tickerData, this);
						break;

					case "BearPriceOscillatorPredicted":
						requestedItem = new BearPriceOscillatorPredicted(_tickerData, this);
						break;

					case "BearRsiCrossoverPredicted":
						requestedItem = new BearRsiCrossoverPredicted(_tickerData, this);
						break;

					case "BearSmaCrossoverPredicted":
						requestedItem = new BearSmaCrossoverPredicted(_tickerData, this);
						break;

					case "BearStochasticsCrossoverPredicted":
						requestedItem = new BearStochasticsCrossoverPredicted(_tickerData, this);
						break;

					case "BearStochasticsFastCrossoverPredicted":
						requestedItem = new BearStochasticsFastCrossoverPredicted(_tickerData, this);
						break;

					case "BearStochRsiCrossoverPredicted":
						requestedItem = new BearStochRsiCrossoverPredicted(_tickerData, this);
						break;

					case "BearTrixSignalCrossoverPredicted":
						requestedItem = new BearTrixSignalCrossoverPredicted(_tickerData, this);
						break;

					case "BearTrixZeroCrossoverPredicted":
						requestedItem = new BearTrixZeroCrossoverPredicted(_tickerData, this);
						break;

					case "BearWilliamsRCrossoverPredicted":
						requestedItem = new BearWilliamsRCrossoverPredicted(_tickerData, this);
						break;

					default:
						throw new Exception(nameAndParameters + " doesn't exist");
				}

				_createdItems[key] = requestedItem;
			}

			return requestedItem;
		}
	}
}
