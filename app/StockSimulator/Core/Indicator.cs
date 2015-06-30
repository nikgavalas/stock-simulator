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
		// Common base so we can have different types of series objects.
		public interface IPlotSeries
		{
		}

		/// <summary>
		/// Class for holding a series of data to be drawn on highcharts.
		/// </summary>
		public class PlotSeries : IPlotSeries
		{
			[JsonProperty("data")]
			public List<List<object>> PlotData { get; set; }

			[JsonProperty("type")]
			public string PlotType { get; set; }

			[JsonProperty("connectNulls")]
			public bool ShouldConnectNulls { get; set; }

			[JsonProperty("dashStyle")]
			public string DashStyle { get; set; }

			/// <summary>
			/// Default constructor
			/// </summary>
			public PlotSeries()
			{
				PlotData = new List<List<object>>();
				PlotType = "line";
				ShouldConnectNulls = false;
				DashStyle = "Solid";
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
		/// Series for flags.
		/// </summary>
		public class PlotSeriesFlag : IPlotSeries
		{
			[JsonProperty("data")]
			public List<Dictionary<string, object>> PlotData { get; set; }

			[JsonProperty("type")]
			public string PlotType { get; set; }

			/// <summary>
			/// Default constructor
			/// </summary>
			public PlotSeriesFlag()
			{
				PlotData = new List<Dictionary<string, object>>();
				PlotType = "flags";
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
		public Dictionary<string, IPlotSeries> ChartPlots { get; set; }

		/// <summary>
		/// Number of bars to look back and simulate from the current bar.
		/// </summary>
		public int MaxSimulationBars { get; set; }

		/// <summary>
		/// Numer of bars to plot on the chart.
		/// </summary>
		public int MaxPlotBars { get; set; }

		/// <summary>
		/// Add this indicator to be saved for output later.
		/// </summary>
		/// <param name="tickerData">Ticker that the indicator is calculated with</param>
		public Indicator(TickerData tickerData) : base(tickerData)
		{
			// Default to about 2 years of lookback data.
			MaxSimulationBars = 500;
			MaxPlotBars = 500;
		}

		/// <summary>
		/// Inits the list so they can be outputted in a json/highcharts friendly way.
		/// This should be overloaded for each indicator.
		/// <param name="endBar">Bar to end the data at.</param>
		/// </summary>
		public void Serialize(int endBar)
		{
			ChartPlots = new Dictionary<string, IPlotSeries>();
			CreatePlots();

			int startBar = Math.Max(0, endBar - MaxPlotBars);
			for (int i = startBar; i <= endBar; i++)
			{
				AddToPlots(i);
			}
		}

		/// <summary>
		/// Creates the plots for the data to be added to.
		/// </summary>
		public virtual void CreatePlots()
		{
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public virtual void AddToPlots(int currentBar)
		{
		}

		/// <summary>
		/// Releases the resources allocated when prepping for serialization.
		/// </summary>
		public void FreeResourcesAfterSerialization()
		{
			ChartPlots = null;
		}

		/// <summary>
		/// Runs the indicator from it's lookback bars till this bar.
		/// </summary>
		/// <param name="bar">Bar to run to</param>
		public void RunToBar(int bar)
		{
			// Run all the dependent indicators first.
			for (int i = 0; i < _dependents.Count; i++)
			{
				if (_dependents[i] is Indicator)
				{
					((Indicator)_dependents[i]).RunToBar(bar);
				}
			}

			int startBar = Math.Max(0, bar - MaxSimulationBars);
			for (int i = startBar; i <= bar; i++)
			{
				OnBarUpdate(i);
			}
		}

		/// <summary>
		/// Called everytime there is a new bar of data. For indicators we are only
		/// allowed to call this from the RunToBar function.
		/// <param name="currentBar">Current bar to simulate</param>
		/// </summary>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);
		}
	}
}
