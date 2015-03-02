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
	/// Sma indicator
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	class Bollinger : Indicator
	{
		private int _period = 14;
		private int _numStdDev = 2;

		public List<double> Upper { get; set; }
		public List<double> Middle { get; set; }
		public List<double> Lower { get; set; }

		/// <summary>
		/// This indicator is plotted on the price bars.
		/// </summary>
		[JsonProperty("plotOnPrice")]
		public override bool PlotOnPrice
		{
			get { return true; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="tickerData">Ticker data for the indicator</param>
		/// <param name="factory">Factory to create the dependent runnables</param>
		public Bollinger(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			Upper = Enumerable.Repeat(0d, Data.NumBars).ToList();
			Middle = Enumerable.Repeat(0d, Data.NumBars).ToList();
			Lower = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Bollinger";
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the data plots.
			PlotSeries plotUpper = new PlotSeries("line");
			PlotSeries plotMiddle = new PlotSeries("line");
			PlotSeries plotLower = new PlotSeries("line");
			ChartPlots[ToString() + "Upper"] = plotUpper;
			ChartPlots[ToString() + "Middle"] = plotMiddle;
			ChartPlots[ToString() + "Lower"] = plotLower;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long ticks = UtilityMethods.UnixTicks(Data.Dates[i]);
				
				plotUpper.PlotData.Add(new List<object>()
				{
					ticks,
					Math.Round(Upper[i], 2)
				});
				plotMiddle.PlotData.Add(new List<object>()
				{
					ticks,
					Math.Round(Middle[i], 2)
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

			double smaValue = UtilityMethods.Sma(Data.Close, currentBar, _period);
			double stdDevValue = UtilityMethods.StdDev(Data.Close, currentBar, _period);
			Upper[currentBar] = smaValue + _numStdDev * stdDevValue;
			Middle[currentBar] = smaValue;
			Lower[currentBar] = smaValue - _numStdDev * stdDevValue;
		}
	}
}
