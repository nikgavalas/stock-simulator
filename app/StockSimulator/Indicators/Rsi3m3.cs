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

		public Rsi3m3(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			Value = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"Rsi3"
				};

				return deps;
			}
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

			Rsi rsi = (Rsi)Dependents[0];
			Value[currentBar] = UtilityMethods.Sma(rsi.Avg, currentBar, 3);
		}
	}
}
