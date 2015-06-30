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
	/// Plots the retracement zones for Internal, alternate, and external price retracements
	/// depending on if we are in wave 5 or wave c of an elliot wave.
	/// </summary>
	class PriceRetracements : Indicator
	{
		public enum ExternalType
		{
			_127 = 0,
			_162,
			Count
		}

		public enum InternalType
		{
			_127 = 0,
			_162,
			_262,
			Count
		}

		public enum AlternateType
		{
			_38 = 0,
			_100Wave1,
			_100Wave3,
			_62,
			Count
		}
		
		public List<double>[] External { get; set; }
		public List<double>[] Internal { get; set; }
		public List<double>[] Alternate { get; set; }



		/// <summary>
		/// Creates the indicator.
		/// Add any dependents here.
		/// </summary>
		/// <param name="tickerData">Price data</param>
		public PriceRetracements(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				new ElliotWaves(tickerData)
			};

			MaxSimulationBars = 100;
			MaxPlotBars = 100;
		}

		/// <summary>
		/// Resets the indicator to it's starting state.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			int count = (int)ExternalType.Count;
			External = new List<double>[count];
			for (int i = 0; i < count; i++)
			{
				External[i] = Enumerable.Repeat(0d, Data.NumBars).ToList();	
			}

			count = (int)InternalType.Count;
			Internal = new List<double>[count];
			for (int i = 0; i < count; i++)
			{
				Internal[i] = Enumerable.Repeat(0d, Data.NumBars).ToList();
			}

			count = (int)AlternateType.Count;
			Alternate = new List<double>[count];
			for (int i = 0; i < count; i++)
			{
				Alternate[i] = Enumerable.Repeat(0d, Data.NumBars).ToList();
			}
		}

		/// <summary>
		/// This indicator is plotted on the price bars.
		/// </summary>
		[JsonProperty("plotOnPrice")]
		public override bool PlotOnPrice
		{
			get { return true; }
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "PriceRetracements";
		}

		/// <summary>
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			// Add the indicator for plotting
			ChartPlots["Ext 1.272"] = new PlotSeries("line") { DashStyle = "ShortDot" };
			ChartPlots["Ext 1.618"] = new PlotSeries("line") { DashStyle = "ShortDot" };
			ChartPlots["Ext 2.618"] = new PlotSeries("line") { DashStyle = "ShortDot" };
			ChartPlots["Alt 1.00(0-1)"] = new PlotSeries("line") { DashStyle = "ShortDot" };
			ChartPlots["Alt 1.00(2-3)"] = new PlotSeries("line") { DashStyle = "ShortDot" };
			ChartPlots["Alt 0.382"] = new PlotSeries("line") { DashStyle = "ShortDot" };
			ChartPlots["Alt 0.618"] = new PlotSeries("line") { DashStyle = "ShortDot" };
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public override void AddToPlots(int currentBar)
		{
			base.AddToPlots(currentBar);

			long ticks = UtilityMethods.UnixTicks(Data.Dates[currentBar]);
			double value = 0.0;

			PlotSeries ext127 = (PlotSeries)ChartPlots["Ext 1.272"];
			value = External[(int)ExternalType._127][currentBar];
			ext127.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			PlotSeries ext162 = (PlotSeries)ChartPlots["Ext 1.618"];
			value = External[(int)ExternalType._162][currentBar];
			ext162.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			PlotSeries alt100Wave1 = (PlotSeries)ChartPlots["Alt 1.00(0-1)"];
			value = Alternate[(int)AlternateType._100Wave1][currentBar];
			alt100Wave1.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			PlotSeries alt100Wave3 = (PlotSeries)ChartPlots["Alt 1.00(2-3)"];
			value = Alternate[(int)AlternateType._100Wave3][currentBar];
			alt100Wave3.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			PlotSeries alt382 = (PlotSeries)ChartPlots["Alt 0.382"];
			value = Alternate[(int)AlternateType._38][currentBar];
			alt382.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			PlotSeries alt618 = (PlotSeries)ChartPlots["Alt 0.618"];
			value = Alternate[(int)AlternateType._62][currentBar];
			alt618.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			ElliotWaves waves = (ElliotWaves)_dependents[0];

			if (waves.IsInFifthWave(currentBar))
			{
				// Get all the wave points to measure retracements and projections.
				List<ElliotWaves.WavePointWithLabel> points = waves.GetWavePoints(currentBar);

				// External retracements are the 4th point - 3rd point, then add that value to the 4th point.
				double externalDiff = points[4].Price - points[3].Price;
				External[(int)ExternalType._127][currentBar] = points[4].Price - externalDiff * 1.272;
				External[(int)ExternalType._162][currentBar] = points[4].Price - externalDiff * 1.618;

				// Alternate retracements for trends are a bit different.
				// The 100% retracements are for 0-1 and 2-3 projected from 4.
				// The 38.2% and 61.8% projects are 0-3 projected from 4.
				double alternateDiff = points[1].Price - points[0].Price;
				Alternate[(int)AlternateType._100Wave1][currentBar] = points[4].Price + alternateDiff;
				alternateDiff = points[3].Price - points[2].Price;
				Alternate[(int)AlternateType._100Wave3][currentBar] = points[4].Price + alternateDiff;
				alternateDiff = points[3].Price - points[0].Price;
				Alternate[(int)AlternateType._38][currentBar] = points[4].Price + alternateDiff * 0.382;
				Alternate[(int)AlternateType._62][currentBar] = points[4].Price + alternateDiff * 0.618;

			}
		}
	}
}
