using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// Hanging Man
	/// </summary>
	class HangingMan : BaseCandlestick
	{
		public HangingMan(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "HangingMan";
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			Trend trend = (Trend)Dependents[0];
			if (Simulator.Config.TrendStrength > 0)
			{
				if (trend.DownTrend[currentBar] == true || UtilityMethods.Max(Data.Low, currentBar, Simulator.Config.TrendStrength) != Data.High[currentBar])
				{
					return;
				}
			}

			if (Data.Low[currentBar] < Data.Open[currentBar] - 5 * Data.TickSize && Math.Abs(Data.Open[currentBar] - Data.Close[currentBar]) < (0.10 * (Data.High[currentBar] - Data.Low[currentBar])) && (Data.High[currentBar] - Data.Close[currentBar]) < (0.25 * (Data.High[currentBar] - Data.Low[currentBar])))
			{
				Found[currentBar] = true;
			}	
		}

	}
}
