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
	class GavalasZones : Indicator
	{
		/// <summary>
		/// Buy zone class
		/// </summary>
		private class PriceZone
		{
			public double High { get; set; }
			public double Low { get; set; }
			public int NumberOfPoints { get; set; }
		}

		/// <summary>
		/// Class that holds a point found on the on the zigzag.
		/// </summary>
		private class ZigZagPoint
		{
			public double Price { get; set; }
			public int Bar { get; set; }
		}

		public enum ExternalType
		{
			_127 = 0,
			_162,
			Count
		}

		public enum InternalType
		{
			_38 = 0,
			_50,
			_62,
			_79,
			Count
		}

		public enum AlternateType
		{
			_62 = 0,
			_100,
			_162,
			Count
		}

		public List<double>[] External { get; set; }
		public List<double>[] Internal { get; set; }
		public List<double>[] Alternate { get; set; }

		public List<double> BuyDirection { get; set; }

		#region Configurables
		public double MaxZonePercent
		{
			get { return _maxZonePercent; }
			set { _maxZonePercent = value; }
		}

		public int MinProjectionsForZone
		{
			get { return _minProjectionsForZone; }
			set { _minProjectionsForZone = value; }
		}

		private double _maxZonePercent = 1.0;
		private int _minProjectionsForZone = 2;
		#endregion

		private int _maxCycleLookback = 100;

		/// <summary>
		/// Creates the indicator.
		/// Add any dependents here.
		/// </summary>
		/// <param name="tickerData">Price data</param>
		public GavalasZones(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				new ZigZag(tickerData, 3.0) { MaxSimulationBars = 150, MaxPlotBars = 150 }
			};

			MaxSimulationBars = 1;
			MaxPlotBars = 1;

			BuyDirection = UtilityMethods.CreateList<double>(Data.NumBars, 0d);

			int count = (int)ExternalType.Count;
			External = new List<double>[count];
			for (int i = 0; i < count; i++)
			{
				External[i] = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			}

			count = (int)InternalType.Count;
			Internal = new List<double>[count];
			for (int i = 0; i < count; i++)
			{
				Internal[i] = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			}

			count = (int)AlternateType.Count;
			Alternate = new List<double>[count];
			for (int i = 0; i < count; i++)
			{
				Alternate[i] = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
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
			return "GavalasZones";
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
			ChartPlots["Alt 0.618"] = new PlotSeries("line") { DashStyle = "ShortDot" };
			ChartPlots["Alt 1.000"] = new PlotSeries("line") { DashStyle = "ShortDot" };
			ChartPlots["Alt 1.618"] = new PlotSeries("line") { DashStyle = "ShortDot" };
			ChartPlots["Int 0.382"] = new PlotSeries("line") { DashStyle = "ShortDot" };
			ChartPlots["Int 0.500"] = new PlotSeries("line") { DashStyle = "ShortDot" };
			ChartPlots["Int 0.618"] = new PlotSeries("line") { DashStyle = "ShortDot" };
			ChartPlots["Int 0.786"] = new PlotSeries("line") { DashStyle = "ShortDot" };
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

			PlotSeries plot = (PlotSeries)ChartPlots["Ext 1.272"];
			value = External[(int)ExternalType._127][currentBar];
			plot.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			plot = (PlotSeries)ChartPlots["Ext 1.618"];
			value = External[(int)ExternalType._162][currentBar];
			plot.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			plot = (PlotSeries)ChartPlots["Alt 0.618"];
			value = Alternate[(int)AlternateType._62][currentBar];
			plot.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			plot = (PlotSeries)ChartPlots["Alt 1.000"];
			value = Alternate[(int)AlternateType._100][currentBar];
			plot.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			plot = (PlotSeries)ChartPlots["Alt 1.618"];
			value = Alternate[(int)AlternateType._162][currentBar];
			plot.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			plot = (PlotSeries)ChartPlots["Int 0.382"];
			value = Internal[(int)InternalType._38][currentBar];
			plot.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			plot = (PlotSeries)ChartPlots["Int 0.500"];
			value = Internal[(int)InternalType._50][currentBar];
			plot.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			plot = (PlotSeries)ChartPlots["Int 0.618"];
			value = Internal[(int)InternalType._62][currentBar];
			plot.PlotData.Add(new List<object>()
			{
				ticks,
				value > 0.0 ? (object)Math.Round(value, 2) : null
			});

			plot = (PlotSeries)ChartPlots["Int 0.786"];
			value = Internal[(int)InternalType._79][currentBar];
			plot.PlotData.Add(new List<object>()
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

			if (currentBar < 2)
			{
				return;
			}

			int numPoints = 4;
			int lastPoint = numPoints - 1;

			ZigZag zigzag = (ZigZag)_dependents[0];
			int cutoffBar = Math.Max(0, currentBar - _maxCycleLookback);
			int searchBar = currentBar - 2;
			ZigZagPoint[] points = new ZigZagPoint[numPoints];
			List<double> currentSeries = null;
			int pointBeingSearchFor = lastPoint;
			double trendDirection = 0.0;

			// Start searching for the point that either made a high-high or low-low cycle.
			for (int i = searchBar; i >= cutoffBar && pointBeingSearchFor >= 0; i--)
			{
				if (pointBeingSearchFor == lastPoint)
				{
					if (zigzag.ZigZagLows[i] > 0.0)
					{
						trendDirection = Order.OrderType.Long;
						points[pointBeingSearchFor] = new ZigZagPoint() { Bar = i, Price = zigzag.Value[i] };
						currentSeries = zigzag.ZigZagHighs;
						--pointBeingSearchFor;
					}
					else if (zigzag.ZigZagHighs[i] > 0.0)
					{
						trendDirection = Order.OrderType.Short;
						points[pointBeingSearchFor] = new ZigZagPoint() { Bar = i, Price = zigzag.Value[i] };
						currentSeries = zigzag.ZigZagLows;
						--pointBeingSearchFor;
					}
				}
				// Each time we find a zigzag point, save the price and move to the next one for searching.
				else if (currentSeries != null && currentSeries[i] > 0.0)
				{
					points[pointBeingSearchFor] = new ZigZagPoint() { Bar = i, Price = zigzag.Value[i] };
					currentSeries = currentSeries == zigzag.ZigZagHighs ? zigzag.ZigZagLows : zigzag.ZigZagHighs;
					--pointBeingSearchFor;
				}
			}

			// Did we get all the points needed to make price and time projections.
			if (pointBeingSearchFor < 0)
			{
				// TODO: do the time projection first, and if the current bar is within the timing
				// zone then we'll do the price projections and save them. So that way all that shows up
				// on the chart are the price projections where the time projections are.

				// External retracements are last point minus the one before projected from the last point.
				double externalDiff = points[lastPoint].Price - points[lastPoint - 1].Price;
				External[(int)ExternalType._127][currentBar] = points[lastPoint].Price - externalDiff * 1.272;
				External[(int)ExternalType._162][currentBar] = points[lastPoint].Price - externalDiff * 1.618;

				// Alternate retracements are the 2nd to last point minus the one right before it projected from the last point.
				double alternateDiff = points[lastPoint - 1].Price - points[lastPoint - 2].Price;
				Alternate[(int)AlternateType._62][currentBar] = points[lastPoint].Price + alternateDiff * 0.618;
				Alternate[(int)AlternateType._100][currentBar] = points[lastPoint].Price + alternateDiff;
				Alternate[(int)AlternateType._162][currentBar] = points[lastPoint].Price + alternateDiff * 1.618;

				// Internal retracements are the 2nd point minus the first point projected from the last point.
				double interalDiff = points[1].Price - points[0].Price;
				Internal[(int)InternalType._38][currentBar] = points[lastPoint].Price - interalDiff * 0.382;
				Internal[(int)InternalType._50][currentBar] = points[lastPoint].Price - interalDiff * 0.500;
				Internal[(int)InternalType._62][currentBar] = points[lastPoint].Price - interalDiff * 0.618;
				Internal[(int)InternalType._79][currentBar] = points[lastPoint].Price - interalDiff * 0.786;

				// The last cycle sets us up for the opposite cycle.
				BuyDirection[currentBar] = trendDirection * -1.0;
			}
			else
			{
				External[(int)ExternalType._127][currentBar] = 0.0;
				External[(int)ExternalType._162][currentBar] = 0.0;
				Alternate[(int)AlternateType._62][currentBar] = 0.0;
				Alternate[(int)AlternateType._100][currentBar] = 0.0;
				Alternate[(int)AlternateType._162][currentBar] = 0.0;
				Internal[(int)InternalType._38][currentBar] = 0.0;
				Internal[(int)InternalType._50][currentBar] = 0.0;
				Internal[(int)InternalType._62][currentBar] = 0.0;
				Internal[(int)InternalType._79][currentBar] = 0.0;
				BuyDirection[currentBar] = 0.0;
			}
		}

		/// <summary>
		/// Returns if the price bar touched one of our buy zones.
		/// </summary>
		/// <param name="lowPrice">Low of the bar to check</param>
		/// <param name="highPrice">High of the bar to check</param>
		/// <param name="barNum">Bar for the price</param>
		/// <returns>True if the bar touched a zone</returns>
		public bool DidBarTouchZone(double lowPrice, double highPrice, int barNum)
		{
			List<PriceZone> zones = GetZones(barNum);
			for (int i = 0; i < zones.Count; i++)
			{
				PriceZone zone = zones[i];
				if ((highPrice >= zone.High && lowPrice <= zone.Low) ||
					(highPrice >= zone.Low && lowPrice <= zone.High))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns a list of all the zones created by all the projections.
		/// </summary>
		/// <param name="barNum">Bar to get the zones for</param>
		/// <returns>See summary</returns>
		private List<PriceZone> GetZones(int barNum)
		{
			List<PriceZone> zones = new List<PriceZone>();
			List<double> points = new List<double>();

			AddPointToListIfValid(points, External[(int)ExternalType._127][barNum]);
			AddPointToListIfValid(points, External[(int)ExternalType._162][barNum]);
			AddPointToListIfValid(points, Alternate[(int)AlternateType._62][barNum]);
			AddPointToListIfValid(points, Alternate[(int)AlternateType._100][barNum]);
			AddPointToListIfValid(points, Alternate[(int)AlternateType._162][barNum]);
			AddPointToListIfValid(points, Internal[(int)InternalType._38][barNum]);
			AddPointToListIfValid(points, Internal[(int)InternalType._50][barNum]);
			AddPointToListIfValid(points, Internal[(int)InternalType._62][barNum]);
			AddPointToListIfValid(points, Internal[(int)InternalType._79][barNum]);

			ComboSet<double> comboSet = new ComboSet<double>(points);
			List<List<double>> combos = comboSet.GetSet(MinProjectionsForZone);

			for (int i = 0; i < combos.Count; i++)
			{
				if (AreAllPointsClose(combos[i]) == true)
				{
					double high = combos[i].Max();
					double low = combos[i].Min();
					zones.Add(new PriceZone() { High = high, Low = low, NumberOfPoints = combos[i].Count });
				}
			}

			// The one with the most similar points is the best. See if it lands there first.
			zones.Sort((a, b) => a.NumberOfPoints.CompareTo(b.NumberOfPoints));

			return zones;
		}

		/// <summary>
		/// Adds a price point to the list if it's > 0.0.
		/// </summary>
		/// <param name="points">List of points</param>
		/// <param name="point">Point to add if it's > 0.0</param>
		private void AddPointToListIfValid(List<double> points, double point)
		{
			if (point > 0.0)
			{
				points.Add(point);
			}
		}

		/// <summary>
		/// Compares a list of points and if they are all within a certain range returns true.
		/// </summary>
		/// <param name="points">List of points to compare</param>
		/// <returns>True if all points are within a specified percent</returns>
		private bool AreAllPointsClose(List<double> points)
		{
			bool closeEnough = true;

			for (int i = 0; i < points.Count; i++)
			{
				for (int j = i + 1; j < points.Count; j++)
				{
					if (Math.Abs(UtilityMethods.PercentChange(points[i], points[j])) > MaxZonePercent)
					{
						closeEnough = false;
						break;
					}
				}
			}

			return closeEnough;
		}

	}
}
