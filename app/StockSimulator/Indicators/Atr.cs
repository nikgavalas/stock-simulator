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
	/// Atr
	/// </summary>
	class Atr : Indicator
	{
		public List<double> Value { get; set; }
		public List<double> ValueNormalized { get; set; }

		#region Configurables
		public int Period
		{
			get { return _period; }
			set { _period = value; }
		}

		private int _period = 14;
		#endregion


		public Atr(TickerData tickerData)
			: base(tickerData)
		{
			Value = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			ValueNormalized = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Atr" + Period.ToString();
		}

		/// <summary>
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			// Add the indicator for plotting
			ChartPlots["AtrNormalized"] = new PlotSeries("line");
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public override void AddToPlots(int currentBar)
		{
			base.AddToPlots(currentBar);

			long ticks = UtilityMethods.UnixTicks(Data.Dates[currentBar]);

			double value = ValueNormalized[currentBar];
			AddValueToPlot("AtrNormalized", ticks, Math.Round(value, 4));
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar == 0)
			{
				Value[currentBar] = Data.High[currentBar] - Data.Low[currentBar];
			}
			else
			{
				double trueRange = Data.High[currentBar] - Data.Low[currentBar];
				trueRange = Math.Max(Math.Abs(Data.Low[currentBar] - Data.Close[currentBar - 1]), Math.Max(trueRange, Math.Abs(Data.High[currentBar] - Data.Close[currentBar - 1])));
				Value[currentBar] = ((Math.Min(currentBar + 1, Period) - 1) * Value[currentBar - 1] + trueRange) / Math.Min(currentBar + 1, Period);
				ValueNormalized[currentBar] = Value[currentBar] / Data.Close[currentBar];
			}
		}
	}
}
