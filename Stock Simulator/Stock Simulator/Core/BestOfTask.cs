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

		public BestOfTask(string symbolName)
		{
			// TODO: Get the data for this symbol.
			DataSeries symbolData = new DataSeries();

			_strategyTree = new StrategyTree("BestOfSubStrategies", symbolData);
		}

		public void RunAll()
		{
			_strategyTree.RunAll();
		}
	}
}
