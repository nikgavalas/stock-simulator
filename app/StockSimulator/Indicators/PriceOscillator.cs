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
	/// Price Oscillator
	/// </summary>
	class PriceOscillator : Indicator
	{
		public List<double> Value { get; set; }

		private int fast = 12;
		private int slow = 26;
		private int smooth = 9;
		private List<double> smoothEma;
		private List<double> smoothDiff;
		private List<double> fastEma;
		private List<double> slowEma;


		public PriceOscillator(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			Value = Enumerable.Repeat(0d, Data.NumBars).ToList();
			smoothEma = Enumerable.Repeat(0d, Data.NumBars).ToList();
			smoothDiff = Enumerable.Repeat(0d, Data.NumBars).ToList();
			fastEma = Enumerable.Repeat(0d, Data.NumBars).ToList();
			slowEma = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "PriceOscillator";
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

			slowEma[currentBar] = UtilityMethods.Ema(Data.Close, slowEma, currentBar, slow);
			fastEma[currentBar] = UtilityMethods.Ema(Data.Close, fastEma, currentBar, fast);
			smoothDiff[currentBar] = fastEma[currentBar] - slowEma[currentBar];
			smoothEma[currentBar] = UtilityMethods.Ema(smoothDiff, smoothEma, currentBar, smooth);
			Value[currentBar] = smoothEma[currentBar];
		}
	}
}
