using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StockSimulator.Core
{
	/// <summary>
	/// Base class for each indicator.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class Indicator : Runnable
	{
		/// <summary>
		/// Class for holding a series of data to be drawn on highcharts.
		/// </summary>
		public class PlotSeries
		{
			[JsonProperty("data")]
			public List<List<object>> PlotData { get; set; }

			[JsonProperty("type")]
			public string PlotType { get; set; }

			/// <summary>
			/// Default constructor
			/// </summary>
			public PlotSeries()
			{
				PlotData = new List<List<object>>();
				PlotType = "line";
			}

			/// <summary>
			/// Constructor that allows setting the type.
			/// </summary>
			/// <param name="plotType">Type of the plot data (line, column, etc)</param>
			public PlotSeries(string plotType)
			{
				PlotData = new List<List<object>>();
				PlotType = plotType;
			}
		}

		/// <summary>
		/// True if the indicator should be drawn in the price area. False if it should have its own area to draw.
		/// </summary>
		[JsonProperty("plotOnPrice")]
		public virtual bool PlotOnPrice
		{
			get { return false; }
		}

		[JsonProperty("series")]
		public Dictionary<string, PlotSeries> ChartPlots { get; set; }

		/// <summary>
		/// Add this indicator to be saved for output later.
		/// </summary>
		/// <param name="tickerData">Ticker that the indicator is calculated with</param>
		/// <param name="factory">Factory for creating runnables</param>
		public Indicator(TickerData tickerData, RunnableFactory factory) : base(tickerData, factory)
		{
		}

		/// <summary>
		/// Inits the list so they can be outputted in a json/highcharts friendly way.
		/// This should be overloaded for each indicator.
		/// </summary>
		public virtual void PrepareForSerialization()
		{
			ChartPlots = new Dictionary<string, PlotSeries>();
		}

		/// <summary>
		/// Releases the resources allocated when prepping for serialization.
		/// </summary>
		public void FreeResourcesAfterSerialization()
		{
			ChartPlots = null;
		}
	}
}
