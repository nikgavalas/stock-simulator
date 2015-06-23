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
	class BearBressertDss : Strategy
	{
		private int _period;

		public BearBressertDss(TickerData tickerData, RunnableFactory factory, string[] settings)
			: base(tickerData, factory)
		{
			_period = Convert.ToInt32(settings[0]);
			_orderType = Order.OrderType.Short;
		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"BressertDss," + _period.ToString()
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
			return "BearBressertDss," + _period;
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

			// The purpose of this strategy is to find the setup bar for the BressertApproach strategy.
			// That strategy will take care of placing the orders.

			// The setup bar is found when the indicator goes above the buyline (40) and then turns down.
			if (DataSeries.IsAbove(ind.Value, 60, currentBar, 2) != -1 && UtilityMethods.IsPeak(ind.Value, currentBar))
			{
				WasFound[currentBar] = true;
			}
		}
	}
}
