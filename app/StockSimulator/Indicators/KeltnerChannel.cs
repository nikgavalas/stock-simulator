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

		#region Configurables
		public int Period
		{
			get { return _period; }
			set { _period = value; }
		}

		public double OffsetMultiplier
		{
			get { return offsetMultiplier; }
			set { offsetMultiplier = value; }
		}

		private int _period = 10;
		private double offsetMultiplier = 1.5;
		#endregion



		public KeltnerChannel(TickerData tickerData)
			: base(tickerData)
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
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			// Add the indicator for plotting
			ChartPlots[ToString() + " Midline"] = new PlotSeries("line");
			ChartPlots[ToString() + " Upper"] = new PlotSeries("line");
			ChartPlots[ToString() + " Lower"] = new PlotSeries("line");
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public override void AddToPlots(int currentBar)
		{
			base.AddToPlots(currentBar);

			long ticks = UtilityMethods.UnixTicks(Data.Dates[currentBar]);

			PlotSeries line = (PlotSeries)ChartPlots[ToString() + " Midline"];
			line.PlotData.Add(new List<object>()
			{
				ticks,
				Math.Round(Midline[currentBar], 2)
			});

			line = (PlotSeries)ChartPlots[ToString() + " Upper"]; 
			line.PlotData.Add(new List<object>()
			{
				ticks,
				Math.Round(Upper[currentBar], 2)
			});

			line = (PlotSeries)ChartPlots[ToString() + " Lower"]; 
			line.PlotData.Add(new List<object>()
			{
				ticks,
				Math.Round(Lower[currentBar], 2)
			});
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
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
