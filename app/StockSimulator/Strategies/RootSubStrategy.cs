using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core;

namespace StockSimulator.Strategies
{
	public class RootSubStrategy : Strategy
	{
		/// <summary>
		/// List of bar data.
		/// </summary>
		public List<BarStatistics> Bars { get; set; }

		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		/// <param name="factory">Factory for creating dependents</param>
		public RootSubStrategy(TickerData tickerData, RunnableFactory factory) 
			: base(tickerData, factory)
		{
			Bars = Enumerable.Repeat(new BarStatistics(), tickerData.NumBars).ToList();
		}
	}
}
