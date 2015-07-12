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
	/// Ppo
	/// </summary>
	class Ppo : Indicator
	{
		public List<double> Value { get; set; }
		public List<double> Smoothed { get; set; }
		public List<double> Diff { get; set; }

		#region Configurables
		public int Fast
		{
			get { return _fast; }
			set { _fast = value; }
		}

		public int Slow
		{
			get { return _slow; }
			set { _slow = value; }
		}

		public int Smooth
		{
			get { return _smooth; }
			set { _smooth = value; }
		}

		private int _fast = 12;
		private int _slow = 26;
		private int _smooth = 9;
		#endregion

		private List<double> _slowEma { get; set; }
		private List<double> _fastEma { get; set; }


		public Ppo(TickerData tickerData)
			: base(tickerData)
		{
			Value = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			Smoothed = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			Diff = UtilityMethods.CreateList<double>(Data.NumBars, 0d);

			_fastEma = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			_slowEma = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Ppo," + Fast.ToString() + "," + Slow.ToString() + "," + Smooth.ToString();
		}

		/// <summary>
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			// Add the indicator for plotting
			ChartPlots["Ppo"] = new PlotSeries("line");
			ChartPlots["PpoSmoothed"] = new PlotSeries("line");
			ChartPlots["PpoDiff"] = new PlotSeries("column");
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public override void AddToPlots(int currentBar)
		{
			base.AddToPlots(currentBar);

			long ticks = UtilityMethods.UnixTicks(Data.Dates[currentBar]);

			double value = Value[currentBar];
			AddValueToPlot("Ppo", ticks, Math.Round(value, 2));

			value = Smoothed[currentBar];
			AddValueToPlot("PpoSmoothed", ticks, Math.Round(value, 2));

			value = Diff[currentBar];
			AddValueToPlot("PpoDiff", ticks, Math.Round(value, 2));
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			_fastEma[currentBar] = UtilityMethods.Ema(Data.Close, _fastEma, currentBar, Fast);
			_slowEma[currentBar] = UtilityMethods.Ema(Data.Close, _slowEma, currentBar, Slow);

			double val = 100 * (_fastEma[currentBar] - _slowEma[currentBar]) / _slowEma[currentBar];
			Value[currentBar] = val;

			Smoothed[currentBar] = UtilityMethods.Ema(Value, Smoothed, currentBar, Smooth);
			Diff[currentBar] = Value[currentBar] - Smoothed[currentBar];
		}
	}
}
