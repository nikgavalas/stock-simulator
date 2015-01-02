using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;

namespace StockSimulator.Indicators
{
	class Sma : Indicator
	{
		private int _period = 4;

		public List<double> Avg { get; set; }

		public Sma(TickerData tickerData, RunnableFactory factory) 
			: base(tickerData, factory)
		{
			Avg = new List<double>(Data.NumBars);
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Sma";
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar == 0)
			{
				Avg.Insert(currentBar, Data.Close[currentBar]);
			}
			else
			{
				double last = Data.Close[currentBar - 1] * Math.Min(currentBar, _period);

				if (currentBar >= _period)
				{
					Avg.Insert(currentBar, (last + Data.Close[currentBar] - Data.Close[currentBar - _period]) / Math.Min(currentBar, _period));
				}
				else
				{
					Avg.Insert(currentBar, (last + Data.Close[currentBar]) / (Math.Min(currentBar, _period) + 1));
				}
			}
		}
	}
}
