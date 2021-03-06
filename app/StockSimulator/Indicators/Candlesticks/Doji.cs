﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// Doji
	/// </summary>
	class Doji : BaseCandlestick
	{
		public Doji(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Doji";
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			Trend trend = (Trend)Dependents[0];

			if (Simulator.Config.TrendStrength > 0 && trend.UpTrend[currentBar] == true)
			{
				return;
			}

			if (Math.Abs(Data.Close[currentBar] - Data.Open[currentBar]) <= (Data.High[currentBar] - Data.Low[currentBar]) * 0.07)
			{
				Found[currentBar] = true;
			}
		}

	}
}
