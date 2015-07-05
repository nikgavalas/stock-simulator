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
	/// Does time projections and shows them on a histogram.
	/// </summary>
	class GavalasHistogram : Indicator
	{
		/// <summary>
		/// Time zone that a cycle reversal is projected in.
		/// </summary>
		private class TimeZone
		{
			public double Start { get; set; }
			public double End { get; set; }
		}

		public List<double> Value { get; set; }

		#region Configurables
		public int BarPadding
		{
			get { return _barPadding; }
			set { _barPadding = value; }
		}

		public double HistogramValue
		{
			get { return _histogramValue; }
			set { _histogramValue = value; }
		}

		private double _histogramValue = 30.0;
		private int _barPadding = 0;
		#endregion


		/// <summary>
		/// Creates the indicator.
		/// Add any dependents here.
		/// </summary>
		/// <param name="tickerData">Price data</param>
		public GavalasHistogram(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				new ZigZagWaves(tickerData)
			};

			MaxSimulationBars = 1;
			MaxPlotBars = 10;

			Value = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "GavalasHistogram";
		}

		/// <summary>
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			// Add the indicator for plotting
			ChartPlots[ToString()] = new PlotSeries("column");
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public override void AddToPlots(int currentBar)
		{
			base.AddToPlots(currentBar);

			long ticks = UtilityMethods.UnixTicks(Data.Dates[currentBar]);

			PlotSeries plot = (PlotSeries)ChartPlots[ToString()];
			plot.PlotData.Add(new List<object>()
			{
				ticks,
				Math.Round(Value[currentBar], 2)
			});
		}

		/// <summary>
		/// Initializes the indicator.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			// Need to reset this every strategy frame so the values don't accumulate.
			Value.Fill(0d);
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar < 2)
			{
				return;
			}

			ZigZagWaves zigzag = (ZigZagWaves)_dependents[0];

			// Did we get all the points needed to make price and time projections.
			if (zigzag.Waves[currentBar] != null)
			{
				ZigZagWaves.WavePoint[] points = zigzag.Waves[currentBar].Points;

				// External retracements are last point minus the one before projected from the last point.
				int externalDiff = points[ZigZagWaves.LAST_POINT].Bar - points[ZigZagWaves.LAST_POINT - 1].Bar;
				AddValueToHistogram(points[ZigZagWaves.LAST_POINT].Bar + Convert.ToInt32(externalDiff * 1.272));
				AddValueToHistogram(points[ZigZagWaves.LAST_POINT].Bar + Convert.ToInt32(externalDiff * 1.618));

				// Alternate retracements are the 2nd to last point minus the one right before it projected from the last point.
				int alternateDiff = points[ZigZagWaves.LAST_POINT - 1].Bar - points[ZigZagWaves.LAST_POINT - 2].Bar;
				AddValueToHistogram(points[ZigZagWaves.LAST_POINT].Bar + Convert.ToInt32(alternateDiff * 0.618));
				AddValueToHistogram(points[ZigZagWaves.LAST_POINT].Bar + alternateDiff);
				AddValueToHistogram(points[ZigZagWaves.LAST_POINT].Bar + Convert.ToInt32(alternateDiff * 1.618));

				// Internal retracements are the 2nd point minus the first point projected from the last point.
				int interalDiff = points[1].Bar - points[0].Bar;
				AddValueToHistogram(points[ZigZagWaves.LAST_POINT].Bar + Convert.ToInt32(interalDiff * 0.382));
				AddValueToHistogram(points[ZigZagWaves.LAST_POINT].Bar + Convert.ToInt32(interalDiff * 0.500));
				AddValueToHistogram(points[ZigZagWaves.LAST_POINT].Bar + Convert.ToInt32(interalDiff * 0.618));
				AddValueToHistogram(points[ZigZagWaves.LAST_POINT].Bar + Convert.ToInt32(interalDiff * 0.786));
			}
		}

		/// <summary>
		/// Adds the projected bar to the histogram with padding.
		/// </summary>
		/// <param name="projectedBar">Bar where a projected price reveral is</param>
		private void AddValueToHistogram(int projectedBar)
		{
			int startBar = projectedBar - BarPadding;
			int endBar = projectedBar + BarPadding;
			for (int i = startBar; i <= endBar; i++)
			{
				if (i < Data.NumBars && i >= 0)
				{
					Value[i] += HistogramValue;
				}
			}
		}
	}
}
