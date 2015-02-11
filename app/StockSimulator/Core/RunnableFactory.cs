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
					case "Macd":
						requestedItem = new Macd(_tickerData, this);
						break;

					case "Rsi14":
						requestedItem = new Rsi(_tickerData, this, 14);
						break;

					case "Sma":
						requestedItem = new Sma(_tickerData, this);
						break;

					// Strategies.
					case "BestOfSubStrategies":
						requestedItem = new BestOfSubStrategies(_tickerData, this);
						break;

					case "MacdStrategy":
						requestedItem = new MacdStrategy(_tickerData, this);
						break;

					case "MacdCrossStrategy":
						requestedItem = new MacdCrossStrategy(_tickerData, this);
						break;

					case "RsiStrategy":
						requestedItem = new RsiStrategy(_tickerData, this);
						break;

					case "SmaStrategy":
						requestedItem = new SmaStrategy(_tickerData, this);
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
