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

		#region Configurables
		public int Period
		{
			get { return _period; }
			set { _period = value; }
		}

		public int Smoothing
		{
			get { return _smoothing; }
			set { _smoothing = value; }
		}

		private int _period = 10;
		private int _smoothing = 15;
		#endregion

		public BressertDss(TickerData tickerData)
			: base(tickerData)
		{
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
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			// Add the indicator for plotting
			PlotSeries line = new PlotSeries("line");
			ChartPlots[ToString()] = line;
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public override void AddToPlots(int currentBar)
		{
			base.AddToPlots(currentBar);

			PlotSeries line = (PlotSeries)ChartPlots[ToString()];
			line.PlotData.Add(new List<object>()
			{
				UtilityMethods.UnixTicks(Data.Dates[currentBar]),
				Math.Round(Value[currentBar], 2)
			});
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
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
