using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;
using Newtonsoft.Json;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// Keltner Channel
	/// </summary>
	class KeltnerChannel : Indicator
	{
		public List<double> Midline { get; set; }
		public List<double> Upper { get; set; }
		public List<double> Lower { get; set; }
		
		private List<double> _diff { get; set; }

		private int _period = 10;
		private double offsetMultiplier = 1.5;

		public KeltnerChannel(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			Midline = Enumerable.Repeat(0d, Data.NumBars).ToList();
			Upper = Enumerable.Repeat(0d, Data.NumBars).ToList();
			Lower = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_diff = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// This indicator is plotted on the price bars.
		/// </summary>
		[JsonProperty("plotOnPrice")]
		public override bool PlotOnPrice
		{
			get { return true; }
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "KeltnerChannel";
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the indicator for plotting
			PlotSeries plotMidline = new PlotSeries("line");
			PlotSeries plotUpper = new PlotSeries("line");
			PlotSeries plotLower = new PlotSeries("line");
			ChartPlots[ToString() + " Midline"] = plotMidline;
			ChartPlots[ToString() + " Upper"] = plotUpper;
			ChartPlots[ToString() + " Lower"] = plotLower;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long ticks = UtilityMethods.UnixTicks(Data.Dates[i]);
				plotMidline.PlotData.Add(new List<object>()
				{
					ticks,
					Math.Round(Midline[i], 2)
				});
				plotUpper.PlotData.Add(new List<object>()
				{
					ticks,
					Math.Round(Upper[i], 2)
				});
				plotLower.PlotData.Add(new List<object>()
				{
					ticks,
					Math.Round(Lower[i], 2)
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

			_diff[currentBar] = Data.High[currentBar] - Data.Low[currentBar];

			double middle = UtilityMethods.Sma(Data.Typical, currentBar, _period);
			double offset = UtilityMethods.Sma(_diff, currentBar, _period) * offsetMultiplier;

			double upper = middle + offset;
			double lower = middle - offset;

			Midline[currentBar] = middle;
			Upper[currentBar] = upper;
			Lower[currentBar] = lower;
		}
	}
}
