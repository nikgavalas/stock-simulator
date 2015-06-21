using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;
using StockSimulator.Indicators;

namespace StockSimulator.Strategies
{
	class BullRsi3m3 : Strategy
	{
		public BullRsi3m3(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"Rsi3m3"
				};

				return deps;
			}
		}

		/// <summary>
		/// Returns the name of this strategy.
		/// </summary>
		/// <returns>The name of this strategy</returns>
		public override string ToString()
		{
			return "BullRsi3m3";
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar < 2)
			{
				return;
			}

			Rsi3m3 rsi = (Rsi3m3)Dependents[0];
			if (DataSeries.IsBelow(rsi.Value, 30, currentBar, 0) != -1 && UtilityMethods.IsValley(rsi.Value, currentBar))
			{
				WasFound[currentBar] = true;
			}
		}
	}
}
