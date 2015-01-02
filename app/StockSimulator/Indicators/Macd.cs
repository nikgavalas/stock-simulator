using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;

namespace StockSimulator.Indicators
{
	class Macd : Indicator
	{
		public Macd(TickerData tickerData, RunnableFactory factory) 
			: base(tickerData, factory)
		{

		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Macd";
		}
	}
}
