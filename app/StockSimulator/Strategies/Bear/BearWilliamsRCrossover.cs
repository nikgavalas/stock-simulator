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
	class BearWilliamsRCrossover : Strategy
	{
		public BearWilliamsRCrossover(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
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
					"WilliamsR"
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
			return "BearWilliamsRCrossover";
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			WilliamsR ind = (WilliamsR)Dependents[0];
			if (DataSeries.CrossBelow(ind.Value, 80, currentBar, 0) != -1)
			{
				WasFound[currentBar] = true;
			}
		}
	}
}
