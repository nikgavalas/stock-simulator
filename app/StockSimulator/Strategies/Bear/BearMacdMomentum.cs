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
	class BearMacdMomentum : Strategy
	{
		public BearMacdMomentum(TickerData tickerData, RunnableFactory factory)
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
					"Macd"
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
			return "BearMacdMomentum";
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

			Macd macd = (Macd)Dependents[0];
			if (macd.Diff[currentBar] > 0 && macd.Diff[currentBar - 1] > 0 && macd.Diff[currentBar - 2] > 0)
			{
				bool leftPeak = macd.Diff[currentBar - 2] < macd.Diff[currentBar - 1];
				bool rightPeak = macd.Diff[currentBar - 1] > macd.Diff[currentBar];
				if (leftPeak == true && rightPeak == true)
				{
					WasFound[currentBar] = true;
				}
			}
		}
	}

}
