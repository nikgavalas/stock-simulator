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
	/// BressertDss
	/// https://www.mql5.com/en/code/8310
	/// </summary>
	public class BressertDss : Indicator
	{
		public List<double> Value { get; set; }

		private List<double> ema;
		private List<double> toBeSmoothed;

		private int _period = 10;
		private int _smoothing = 15;

		public BressertDss(TickerData tickerData, RunnableFactory factory, string[] settings)
			: base(tickerData, factory)
		{
			_period = Convert.ToInt32(settings[0]);

			Value = Enumerable.Repeat(0d, Data.NumBars).ToList();
			ema = Enumerable.Repeat(0d, Data.NumBars).ToList();
			toBeSmoothed = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "BressertDss," + _period.ToString();
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the rsi for plotting
			PlotSeries plotValue = new PlotSeries("line");
			ChartPlots[ToString()] = plotValue;

			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long ticks = UtilityMethods.UnixTicks(Data.Dates[i]);
				plotValue.PlotData.Add(new List<object>()
				{
					ticks,
					Math.Round(Value[i], 2)
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

			double periodLow = UtilityMethods.Min(Data.Close, currentBar, _period);
			double periodHigh = UtilityMethods.Max(Data.Close, currentBar, _period);
			double denominator = periodHigh - periodLow;

			toBeSmoothed[currentBar] = denominator > 0.0 ? ((Data.Close[currentBar] - periodLow) / denominator) * 100.0 : 0.0;
			ema[currentBar] = UtilityMethods.Ema(toBeSmoothed, ema, currentBar, _smoothing);

			Value[currentBar] = ema[currentBar];
		}
	}
}
