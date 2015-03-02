using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// Base candlestick class which holds things that all candlesticks need.
	/// This class is meant to be derived.
	/// </summary>
	class BaseCandlestick : Indicator
	{
		public List<bool> Found { get; set; }

		public BaseCandlestick(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			Found = Enumerable.Repeat(false, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"Trend"
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
			return "Candlestick";
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the rsi for plotting
			PlotSeries plot = new PlotSeries("column");
			ChartPlots[ToString()] = plot;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				plot.PlotData.Add(new List<object>()
				{
					UtilityMethods.UnixTicks(Data.Dates[i]),
					Found[i] ? 1 : 0
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
		}

	}
}
