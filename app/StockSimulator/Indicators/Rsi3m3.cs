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
	public class Rsi3m3 : Indicator
	{
		public List<double> Value { get; set; }

		public Rsi3m3(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				(Runnable)new Rsi(Data) { Period = 3 }
			};

			Value = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Rsi3m3";
		}

		/// <summary>
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			// Add the indicator for plotting
			ChartPlots[ToString()] = new PlotSeries("line");
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
			AddValueToPlot(ToString(), ticks, Math.Round(value, 2));
		}


		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			Rsi rsi = (Rsi)_dependents[0];
			Value[currentBar] = UtilityMethods.Sma(rsi.Avg, currentBar, 3);
		}
	}
}
