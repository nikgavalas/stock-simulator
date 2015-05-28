using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// Ease of movement
	/// </summary>
	class EaseOfMovement : Indicator
	{
		public List<double> Value { get; set; }

		private int smoothing = 4;
		private int volumeDivisor = 10000;
		public List<double> emv;

		public EaseOfMovement(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			Value = Enumerable.Repeat(0d, Data.NumBars).ToList();
			emv = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "EaseOfMovement";
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the rsi for plotting
			PlotSeries plot = new PlotSeries("line");
			ChartPlots[ToString()] = plot;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				plot.PlotData.Add(new List<object>()
				{
					UtilityMethods.UnixTicks(Data.Dates[i]),
					Math.Round(Value[i], 6)
				});
			}
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar > 0)
			{
				double midPoint = Data.Median[currentBar] - Data.Median[currentBar - 1];
				double boxRatio = (Data.Volume[currentBar] / volumeDivisor) / (Data.High[currentBar] - Data.Low[currentBar]);

				emv[currentBar] = midPoint / boxRatio;
				Value[currentBar] = UtilityMethods.Ema(emv, Value, currentBar, smoothing);
			}
		}
	}
}
