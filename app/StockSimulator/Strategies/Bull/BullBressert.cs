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
	class BullBressert : Strategy
	{
		public BullBressert(TickerData tickerData, RunnableFactory factory)
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
					"BressertDss",
					"BressertTimingBands"
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
			return "BullBressert";
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

			BressertDss ind = (BressertDss)Dependents[0];
			BressertTimingBands timingBands = (BressertTimingBands)Dependents[1];
			
			// The purpose of this strategy is to find the setup bar for the BressertApproach strategy.
			// That strategy will take care of placing the orders.

			// The setup bar is found when the indicator drops below the buyline (40) and then turns up.
			if (DataSeries.IsBelow(ind.Value, 40, currentBar, 0) != -1 && UtilityMethods.IsValley(ind.Value, currentBar))
			{
				// Make sure this signal occured within a timing band.
				if (timingBands.LowCycle[currentBar] == true)
				{
					WasFound[currentBar] = true;
				}
			}
		}
	}
}
