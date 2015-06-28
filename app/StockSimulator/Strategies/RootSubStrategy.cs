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
		public List<OrderSuggestion> Bars { get; set; }

		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		public RootSubStrategy(TickerData tickerData) 
			: base(tickerData)
		{
		}

		/// <summary>
		/// Resets the indicator to it's starting state.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			Bars = Enumerable.Repeat(new OrderSuggestion(), Data.NumBars).ToList();
		}
	}
}
