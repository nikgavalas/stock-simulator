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
	class UpsideTasukiGapFound : Strategy
	{
		public UpsideTasukiGapFound(TickerData tickerData, RunnableFactory factory)
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
					"UpsideTasukiGap"
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
			return "UpsideTasukiGapFound";
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			UpsideTasukiGap cs = (UpsideTasukiGap)Dependents[0];
			WasFound[currentBar] = cs.Found[currentBar];
		}
	}
}
