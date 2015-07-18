using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;
using Newtonsoft.Json;
using System.Windows;

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
		public class PriceZone
		{
			public double High { get; set; }
			public double Low { get; set; }
			public int NumberOfPoints { get; set; }
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

		public List<PriceZone> HitZone { get; set; }

		public List<double>[] External { get; set; }
		public List<double>[] Internal { get; set; }
		public List<double>[] Alternate { get; set; }

		public List<double> BuyDirection { get; set; }

		public List<double> HighBestFitLine { get; set; }
		public List<double> LowBestFitLine { get; set; }
		public List<double> AllBestFitLine { get; set; }
		public List<double> HighBestFitLineSlope { get; set; }
		public List<double> LowBestFitLineSlope { get; set; }
		public List<double> AllBestFitLineSlope { get; set; }

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
				new ZigZagWaves(tickerData)
			};

			MaxSimulationBars = 1;
			MaxPlotBars = 150;

			HitZone = UtilityMethods.CreateList<PriceZone>(Data.NumBars, null);

			BuyDirection = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			
			HighBestFitLine = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			LowBestFitLine = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			AllBestFitLine = UtilityMethods.CreateList<double>(Data.NumBars, 0d);

			HighBestFitLineSlope = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			LowBestFitLineSlope = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			AllBestFitLineSlope = UtilityMethods.CreateList<double>(Data.NumBars, 0d);

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
			ChartPlots["Ext 1.272"] = new PlotSeries("line") { DashStyle = "ShortDot", ShouldConnectNulls = true };
			ChartPlots["Ext 1.618"] = new PlotSeries("line") { DashStyle = "ShortDot", ShouldConnectNulls = true };
			ChartPlots["Alt 0.618"] = new PlotSeries("line") { DashStyle = "ShortDot", ShouldConnectNulls = true };
			ChartPlots["Alt 1.000"] = new PlotSeries("line") { DashStyle = "ShortDot", ShouldConnectNulls = true };
			ChartPlots["Alt 1.618"] = new PlotSeries("line") { DashStyle = "ShortDot", ShouldConnectNulls = true };
			ChartPlots["Int 0.382"] = new PlotSeries("line") { DashStyle = "ShortDot", ShouldConnectNulls = true };
			ChartPlots["Int 0.500"] = new PlotSeries("line") { DashStyle = "ShortDot", ShouldConnectNulls = true };
			ChartPlots["Int 0.618"] = new PlotSeries("line") { DashStyle = "ShortDot", ShouldConnectNulls = true };
			ChartPlots["Int 0.786"] = new PlotSeries("line") { DashStyle = "ShortDot", ShouldConnectNulls = true };

			ChartPlots["High Best Fit"] = new PlotSeries("line") { ShouldConnectNulls = true };
			ChartPlots["Low Best Fit"] = new PlotSeries("line") { ShouldConnectNulls = true };
			ChartPlots["All Best Fit"] = new PlotSeries("line") { ShouldConnectNulls = true };
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

			value = External[(int)ExternalType._127][currentBar];
			AddValueToPlot("Ext 1.272", ticks, value > 0.0 ? (object)Math.Round(value, 2) : null);

			value = External[(int)ExternalType._162][currentBar];
			AddValueToPlot("Ext 1.618", ticks, value > 0.0 ? (object)Math.Round(value, 2) : null);

			value = Alternate[(int)AlternateType._62][currentBar];
			AddValueToPlot("Alt 0.618", ticks, value > 0.0 ? (object)Math.Round(value, 2) : null);

			value = Alternate[(int)AlternateType._100][currentBar];
			AddValueToPlot("Alt 1.000", ticks, value > 0.0 ? (object)Math.Round(value, 2) : null);

			value = Alternate[(int)AlternateType._162][currentBar];
			AddValueToPlot("Alt 1.618", ticks, value > 0.0 ? (object)Math.Round(value, 2) : null);

			value = Internal[(int)InternalType._38][currentBar];
			AddValueToPlot("Int 0.382", ticks, value > 0.0 ? (object)Math.Round(value, 2) : null);

			value = Internal[(int)InternalType._50][currentBar];
			AddValueToPlot("Int 0.500", ticks, value > 0.0 ? (object)Math.Round(value, 2) : null);

			value = Internal[(int)InternalType._62][currentBar];
			AddValueToPlot("Int 0.618", ticks, value > 0.0 ? (object)Math.Round(value, 2) : null);

			value = Internal[(int)InternalType._79][currentBar];
			AddValueToPlot("Int 0.786", ticks, value > 0.0 ? (object)Math.Round(value, 2) : null);

			value = HighBestFitLine[currentBar];
			AddValueToPlot("High Best Fit", ticks, value > 0.0 ? (object)Math.Round(value, 2) : null);

			value = LowBestFitLine[currentBar];
			AddValueToPlot("Low Best Fit", ticks, value > 0.0 ? (object)Math.Round(value, 2) : null);

			value = AllBestFitLine[currentBar];
			AddValueToPlot("All Best Fit", ticks, value > 0.0 ? (object)Math.Round(value, 2) : null);
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

			// Zero out all the values for the plot.
			for (int i = currentBar; i >= currentBar - MaxPlotBars && i >= 0; i--)
			{
				HitZone[i] = null;

				External[(int)ExternalType._127][i] = 0.0;
				External[(int)ExternalType._162][i] = 0.0;
				Alternate[(int)AlternateType._62][i] = 0.0;
				Alternate[(int)AlternateType._100][i] = 0.0;
				Alternate[(int)AlternateType._162][i] = 0.0;
				Internal[(int)InternalType._38][i] = 0.0;
				Internal[(int)InternalType._50][i] = 0.0;
				Internal[(int)InternalType._62][i] = 0.0;
				Internal[(int)InternalType._79][i] = 0.0;
			
				BuyDirection[i] = 0.0;

				HighBestFitLine[i] = 0.0;
				LowBestFitLine[i] = 0.0;
				AllBestFitLine[i] = 0.0;
				HighBestFitLineSlope[i] = 0.0;
				LowBestFitLineSlope[i] = 0.0;
				AllBestFitLineSlope[i] = 0.0;
			}

			ZigZagWaves zigzag = (ZigZagWaves)_dependents[0];

			// Did we get all the points needed to make price and time projections.
			if (zigzag.Waves[currentBar] != null)
			{
				List<ZigZagWaves.WavePoint> points = zigzag.Waves[currentBar].Points;
				if (DoWavesPassFilters(points))
				{
					// Points indicies are ordered in the first point is the closest point
					// to the current bar.

					// Loop twice purely to display the line as a flat line and we need at least
					// two points for the line to show up on the chart.
					for (int i = currentBar; i >= currentBar - 1; i--)
					{
						// External retracements are last point minus the one before projected from the last point.
						double externalDiff = points[0].Price - points[1].Price;
						External[(int)ExternalType._127][i] = points[0].Price - externalDiff * 1.272;
						External[(int)ExternalType._162][i] = points[0].Price - externalDiff * 1.618;

						// Alternate retracements are the 2nd to last point minus the one right before it projected from the last point.
						double alternateDiff = points[1].Price - points[2].Price;
						Alternate[(int)AlternateType._62][i] = points[0].Price + alternateDiff * 0.618;
						Alternate[(int)AlternateType._100][i] = points[0].Price + alternateDiff;
						Alternate[(int)AlternateType._162][i] = points[0].Price + alternateDiff * 1.618;

						// Internal retracements are the 2nd to last point minus the 3rd to last point projected from the last point.
						double interalDiff = points[2].Price - points[3].Price;
						Internal[(int)InternalType._38][i] = points[0].Price - interalDiff * 0.382;
						Internal[(int)InternalType._50][i] = points[0].Price - interalDiff * 0.500;
						Internal[(int)InternalType._62][i] = points[0].Price - interalDiff * 0.618;
						Internal[(int)InternalType._79][i] = points[0].Price - interalDiff * 0.786;
					}

					// The last cycle sets us up for the opposite cycle.
					BuyDirection[currentBar] = zigzag.Waves[currentBar].TrendDirection * -1.0;

					// Save the best fit lines for this set of waves.
					CalculateLinesOfBestFit(points, currentBar);
				}
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
					HitZone[barNum] = zone;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns the wave data for the current zigzag waves.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <returns>See summary</returns>
		public ZigZagWaves.WaveData GetWaveData(int currentBar)
		{
			ZigZagWaves zigzag = (ZigZagWaves)_dependents[0];
			return zigzag.Waves[currentBar];
		}

		/// <summary>
		/// Sets the zigzag deviation value.
		/// </summary>
		/// <param name="deviation">New deviation value</param>
		public void SetZigZagDeviation(double deviation)
		{
			ZigZagWaves zigzag = (ZigZagWaves)_dependents[0];
			zigzag.SetZigZagDeviation(deviation);
		}

		/// <summary>
		/// Calculates lines of best fit for the past highs and lows.
		/// </summary>
		/// <param name="points">List of points to calculate from</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		private void CalculateLinesOfBestFit(List<ZigZagWaves.WavePoint> points, int currentBar)
		{
			int highMod = BuyDirection[currentBar] > 0.0 ? 0 : 1;
			int lowMod = highMod ^ 1;

			ZigZagWaves zigzag = (ZigZagWaves)_dependents[0];
			int maxPoints = zigzag.MinRequiredPoints;

			// Get the highs and lows as a list of points to use for the generation.
			List<ZigZagWaves.WavePoint> highs = points.Where((item, index) => index % 2 == highMod && index < maxPoints).ToList();
			List<ZigZagWaves.WavePoint> lows = points.Where((item, index) => index % 2 == lowMod && index < maxPoints).ToList();

			// Also do a line for all the points.
			List<ZigZagWaves.WavePoint> all = points.Where((item, index) => index < maxPoints).ToList();

			// Reverse the order of the points since the lowest index is the last point which will 
			// mess up the slope calculations.
			highs.Reverse();
			lows.Reverse();
			all.Reverse();

			// Save the lines to the values to be plotted.
			SaveLineOfBestFit(HighBestFitLine, highs, HighBestFitLineSlope, currentBar);
			SaveLineOfBestFit(LowBestFitLine, lows, LowBestFitLineSlope, currentBar);
			SaveLineOfBestFit(AllBestFitLine, all, AllBestFitLineSlope, currentBar);
		}

		private void SaveLineOfBestFit(List<double> pointSeries, List<ZigZagWaves.WavePoint> points, List<double> slopeSeries, int currentBar)
		{
			double lineSlope;
			List<ZigZagWaves.WavePoint> generatedPoints = GenerateLinearBestFit(points, currentBar, out lineSlope);

			for (int i = 0; i < generatedPoints.Count; i++)
			{
				pointSeries[generatedPoints[i].Bar] = generatedPoints[i].Price;
			}

			slopeSeries[currentBar] = lineSlope;
		}

		/// <summary>
		/// Generates a line of best fit from a list zigzag points. Also generates a point for the current bar
		/// so we can see the line projected farther on the chart.
		/// http://stackoverflow.com/questions/12946341/algorithm-for-scatter-plot-best-fit-line
		/// </summary>
		/// <param name="points">List of zigzag points</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <param name="lineSlope">Pass out the line slope to be saved if we want</param>
		/// <returns>List of points along the line of best fit</returns>
		private List<ZigZagWaves.WavePoint> GenerateLinearBestFit(List<ZigZagWaves.WavePoint> points, int currentBar, out double lineSlope)
		{
			int numPoints = points.Count;
			double meanX = points.Average(point => point.Bar);
			double meanY = points.Average(point => point.Price);

			double sumXSquared = points.Sum(point => point.Bar * point.Bar);
			double sumXY = points.Sum(point => point.Bar * point.Price);

			double a = (sumXY / numPoints - meanX * meanY) / (sumXSquared / numPoints - meanX * meanX);
			double b = (a * meanX - meanY);

			lineSlope = a;

			List<ZigZagWaves.WavePoint> bestFitPoints = points.Select(point => new ZigZagWaves.WavePoint() { Bar = point.Bar, Price = a * point.Bar - b }).ToList();

			// Add the current bar point.
			bestFitPoints.Add(new ZigZagWaves.WavePoint() { Bar = currentBar, Price = a * currentBar - b });

			return bestFitPoints;
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

		/// <summary>
		/// Checks the found waves to make sure they are optimal for the strategy to work.
		/// </summary>
		/// <param name="points">Array of points making up the waves</param>
		/// <returns>True if the waves will work for our strategy</returns>
		private bool DoWavesPassFilters(List<ZigZagWaves.WavePoint> points)
		{
			return AreWavesGreaterThanMinBars(points);
			//	AreWaveAnglesInRange(points);
		}

		/// <summary>
		/// Returns true if all the waves are greater than the min required bar.
		/// </summary>
		/// <param name="points">Array of points making up the waves</param>
		/// <returns>See Summary</returns>
		private bool AreWavesGreaterThanMinBars(List<ZigZagWaves.WavePoint> points)
		{
			// Check just the last wave for now.
			//if (points[0].Bar - points[1].Bar < 3 || points[1].Bar - points[2].Bar < 3)
			//{
			//	return false;
			//}

			//for (int i = 1; i < points.Count && i < 4; i++)
			//{
			//	int length = points[i].Bar - points[i - 1].Bar;
			//	if (length < 2)
			//	{
			//		return false;
			//	}
			//}

			return true;
		}

		/// <summary>
		/// Returns if the angle between all the waves is in an acceptable range.
		/// </summary>
		/// <param name="points">Array of points making up the waves</param>
		/// <returns>See Summary</returns>
		private bool AreWaveAnglesInRange(List<ZigZagWaves.WavePoint> points)
		{
			double lastWaveAngle = UtilityMethods.CalculateAngle(points[0].Price, points[1].Price, points[2].Price, points[0].Bar, points[1].Bar, points[2].Bar);
			double firstWaveAngle = UtilityMethods.CalculateAngle(points[1].Price, points[2].Price, points[3].Price, points[1].Bar, points[2].Bar, points[3].Bar);

			if (lastWaveAngle > 160 || lastWaveAngle < 20 || firstWaveAngle > 160 || firstWaveAngle < 20)
			{
				return false;
			}

			// Make sure the waves aren't too different from each other. Like a steep wave then a really
			// shallow wave with not much price change.
			if (Math.Abs(lastWaveAngle - firstWaveAngle) > 50)
			{
				return false;
			}

			return true;
		}
	}
}
