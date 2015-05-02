using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// Falling Three Methods
	/// </summary>
	class FallingThreeMethods : BaseCandlestick
	{
		public FallingThreeMethods(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "FallingThreeMethods";
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			Trend trend = (Trend)Dependents[0];

			if (currentBar < 5)
			{
				return;
			}

			if (Data.Close[currentBar - 4] < Data.Open[currentBar - 4] && Data.Close[currentBar] < Data.Open[currentBar] && Data.Close[currentBar] < Data.Low[currentBar - 4]
				&& Data.High[currentBar - 3] < Data.High[currentBar - 4] && Data.Low[currentBar - 3] > Data.Low[currentBar - 4]
				&& Data.High[currentBar - 2] < Data.High[currentBar - 4] && Data.Low[currentBar - 2] > Data.Low[currentBar - 4]
				&& Data.High[currentBar - 1] < Data.High[currentBar - 4] && Data.Low[currentBar - 1] > Data.Low[currentBar - 4])
			{
				Found[currentBar] = true;
			}
		}

	}
}
