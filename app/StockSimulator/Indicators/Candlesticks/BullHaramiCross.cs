using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// Bullish Harami Cross
	/// </summary>
	class BullHaramiCross : BaseCandlestick
	{
		public BullHaramiCross(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "BullHaramiCross";
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			Trend trend = (Trend)Dependents[0];

			if (currentBar < 1 || (Simulator.Config.TrendStrength > 0 && trend.UpTrend[currentBar] == true))
			{
				return;
			}

			if ((Data.High[currentBar] <= Data.Open[currentBar - 1]) && (Data.Low[currentBar] >= Data.Close[currentBar - 1]) && Data.Open[currentBar] >= Data.Close[currentBar - 1] && Data.Close[currentBar] <= Data.Open[currentBar - 1] && ((Data.Close[currentBar] >= Data.Open[currentBar] && Data.Close[currentBar] <= Data.Open[currentBar] + Data.TickSize) || (Data.Close[currentBar] <= Data.Open[currentBar] && Data.Close[currentBar] >= Data.Open[currentBar] - Data.TickSize)))
			{
				Found[currentBar] = true;
			}
		}

	}
}
