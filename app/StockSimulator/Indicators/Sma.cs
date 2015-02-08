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
	class Sma : Indicator
	{
		private int _period = 14;

		public List<double> Avg { get; set; }

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
		public Sma(TickerData tickerData, RunnableFactory factory) 
			: base(tickerData, factory)
		{
			Avg = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Sma";
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the sma avg to the the data for plotting.
			PlotSeries smaPlot = new PlotSeries("line");
			ChartPlots["Sma"] = smaPlot; 
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				smaPlot.PlotData.Add(new List<object>()
				{
					ExtensionMethods.UnixTicks(Data.Dates[i]),
					Avg[i]
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

			if (currentBar == 0)
			{
				Avg[currentBar] = Data.Close[currentBar];
			}
			else
			{
				double last = Data.Close[currentBar - 1] * Math.Min(currentBar, _period);

				if (currentBar >= _period)
				{
					Avg[currentBar] = (last + Data.Close[currentBar] - Data.Close[currentBar - _period]) / Math.Min(currentBar, _period);
				}
				else
				{
					Avg[currentBar] = (last + Data.Close[currentBar]) / (Math.Min(currentBar, _period) + 1);
				}
			}
		}
	}
}
