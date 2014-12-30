using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// Container for the best of strategy task to run it for the dates specified. It can then hold
	/// the value of it for each date just like an indicator or strategy.
	/// </summary>
	class BestOfTask
	{
		private StrategyTree _strategyTree;

		public BestOfTask(TickerExchangePair ticker, TickerDataStore dataStore)
		{
			// Get the data for the symbol.
			TickerData tickerData = dataStore.GetTickerData(ticker, new DateTime(2012, 1, 1), DateTime.Now);

			_strategyTree = new StrategyTree("BestOfSubStrategies", tickerData);
		}

		public void RunAll()
		{
			_strategyTree.RunAll();

			// After they are all simulated, find the highest strategy for each day.
		}
	}
}
