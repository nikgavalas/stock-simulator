﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// Stick Sandwitch
	/// </summary>
	class StickSandwitch : BaseCandlestick
	{
		public StickSandwitch(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "StickSandwitch";
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			Trend trend = (Trend)Dependents[0];

			if (currentBar < 2)
			{
				return;
			}

			if (Data.Close[currentBar - 2] == Data.Close[currentBar] && Data.Close[currentBar - 2] < Data.Open[currentBar - 2] && Data.Close[currentBar - 1] > Data.Open[currentBar - 1] && Data.Close[currentBar] < Data.Open[currentBar])
			{
				Found[currentBar] = true;
			}
		}

	}
}
