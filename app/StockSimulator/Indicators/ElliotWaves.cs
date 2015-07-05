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
	/// Labels the time that we know we are in a 5th wave.
	/// TODO: label the wave number points and also find corrections
	/// </summary>
	class ElliotWaves : Indicator
	{
		// Identfies a wave point with a string id at a price level.
		public class WavePointWithLabel : WavePoint
		{
			public string Label { get; set; }
		}

		/// <summary>
		/// Class that holds a point found on the wave.
		/// </summary>
		public class WavePoint
		{
			public double Price { get; set; }
			public int Bar { get; set; }
		}

		public List<double> FifthWaveValue { get; set; }
		public List<double> FifthWaveDirection { get; set; }
		public List<WavePointWithLabel> WaveLabels { get; set; }

		private int _maxBarsForWave = 500;

		/// <summary>
		/// Creates the indicator.
		/// Add any dependents here.
		/// </summary>
		/// <param name="tickerData">Price data</param>
		public ElliotWaves(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				(Runnable)new ZigZag(Data, 5.0)
			};

			MaxSimulationBars = 1;
		}

		/// <summary>
		/// Resets the indicator to it's starting state.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			FifthWaveValue = Enumerable.Repeat(0d, Data.NumBars).ToList();
			FifthWaveDirection = Enumerable.Repeat(0d, Data.NumBars).ToList();
			WaveLabels = Enumerable.Repeat<WavePointWithLabel>(null, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns true if we think we are currently in the 5th wave of a 5 wave trend.
		/// </summary>
		/// <param name="currentBar">Current bar to check</param>
		/// <returns>See summary</returns>
		public bool IsInFifthWave(int currentBar)
		{
			return FifthWaveValue[currentBar] != 0.0;
		}

		/// <summary>
		/// Returns the wave points for the most recent wave.
		/// </summary>
		/// <param name="currentBar">Bar to get the waves back from</param>
		/// <returns>The wave points for the most recent wave</returns>
		public List<WavePointWithLabel> GetWavePoints(int currentBar)
		{
			int cutoffBar = Math.Max(0, currentBar - _maxBarsForWave);
			List<WavePointWithLabel> points = Enumerable.Repeat<WavePointWithLabel>(null, 6).ToList();
			for (int i = currentBar; i >= cutoffBar; i--)
			{
				if (WaveLabels[i] != null)
				{
					int wavePointNum = Convert.ToInt32(WaveLabels[i].Label);
					points[wavePointNum] = WaveLabels[i];
				}
			}

			return points;
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
			return "ElliotWaves";
		}

		/// <summary>
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			PlotSeries plot = new PlotSeries("line");
			PlotSeriesFlag plotFlag = new PlotSeriesFlag();
			ChartPlots[ToString()] = plot;
			ChartPlots[ToString() + "label"] = plotFlag;
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public override void AddToPlots(int currentBar)
		{
			base.AddToPlots(currentBar);

			PlotSeries plot = (PlotSeries)ChartPlots[ToString()];
			PlotSeriesFlag plotFlag = (PlotSeriesFlag)ChartPlots[ToString() + "label"];

			long ticks = UtilityMethods.UnixTicks(Data.Dates[currentBar]);
			plot.PlotData.Add(new List<object>()
			{
				ticks,
				FifthWaveValue[currentBar] > 0.0 ? (object)Math.Round(FifthWaveValue[currentBar], 2) : null
			});

			if (WaveLabels[currentBar] != null)
			{
				plotFlag.PlotData.Add(new Dictionary<string, object>()
				{
					{ "x", ticks },
					{ "title", WaveLabels[currentBar].Label } 
				});
			}
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar > 1)
			{
				ZigZag zigzag = (ZigZag)_dependents[0];

				int cutoffBar = Math.Max(0, currentBar - _maxBarsForWave);
				int searchBar = currentBar - 2;
				WavePoint[] points = new WavePoint[6];
				WavePointWithLabel[] labels = new WavePointWithLabel[6];
				List<double> currentSeries = null;
				int waveEndpointBeingSearchFor = 4;
				double trendDirection = 0.0;
				double fifthWaveDirection = 0.0; // 1 is bull, -1 bear, 0 not in a 5th wave


				// We have an endpoint on the 4th wave, so lets check and see if we
				// have all the previous waves by searching for the endpoints.
				for (int i = searchBar; i >= cutoffBar && waveEndpointBeingSearchFor >= 0; i--)
				{
					// See if we've just recently had the end of the 4th wave. For bull
					// waves this will start with a low and for bear will start with a high.
					// Need to check the past 3rd bar since that is the lag of the zigzag indicator.
					// For every zig there is a zag meaning if we found a low, the previous point will
					// be a high. This makes it easier to search back for the patterns.
					if (waveEndpointBeingSearchFor == 4)
					{
						if (zigzag.ZigZagLows[i] > 0.0)
						{
							trendDirection = Order.OrderType.Long;
							points[waveEndpointBeingSearchFor] = new WavePoint() { Bar = i, Price = zigzag.Value[i] };
							labels[waveEndpointBeingSearchFor] = new WavePointWithLabel() { Bar = i, Price = zigzag.Value[i], Label = waveEndpointBeingSearchFor.ToString() };
							currentSeries = zigzag.ZigZagHighs;
							--waveEndpointBeingSearchFor;
						}
						else if (zigzag.ZigZagHighs[i] > 0.0)
						{
							trendDirection = Order.OrderType.Short;
							points[waveEndpointBeingSearchFor] = new WavePoint() { Bar = i, Price = zigzag.Value[i] };
							labels[waveEndpointBeingSearchFor] = new WavePointWithLabel() { Bar = i, Price = zigzag.Value[i], Label = waveEndpointBeingSearchFor.ToString() };
							currentSeries = zigzag.ZigZagLows;
							--waveEndpointBeingSearchFor;
						}
					}
					// Each time we find a wave endpoint, save the price and move to the next one for searching.
					else if (currentSeries != null && currentSeries[i] > 0.0)
					{
						points[waveEndpointBeingSearchFor] = new WavePoint() { Bar = i, Price = zigzag.Value[i] };
						labels[waveEndpointBeingSearchFor] = new WavePointWithLabel() { Bar = i, Price = zigzag.Value[i], Label = waveEndpointBeingSearchFor.ToString() };
						currentSeries = currentSeries == zigzag.ZigZagHighs ? zigzag.ZigZagLows : zigzag.ZigZagHighs;
						--waveEndpointBeingSearchFor;
					}
				}

				// Did we get all the points needed to search for the 5 wave trend pattern?
				if (waveEndpointBeingSearchFor < 0)
				{
					// Trend pattern guidelines.
					// 1. Wave-2 cannot trade beyond the beginning of Wave-1
					// 2. Wave-3 cannot be the shortest in price of Waves 1,3, and 5 (but we haven't found
					// this wave yet so we won't check for that.
					// 3. Wave-4 cannot make a daily close into the closing range of Wave-1
					bool isValidTrend = true;

					// Guideline 1.
					if (UtilityMethods.ComparePrices(points[0].Price, points[2].Price, trendDirection) < 0.0)
					{
						isValidTrend = false;
					}

					// Guideline 2.
					double wave1PriceLength = Math.Abs(points[1].Price - points[0].Price);
					double wave3PriceLength = Math.Abs(points[3].Price - points[2].Price);
					if (wave3PriceLength < wave1PriceLength)
					{
						isValidTrend = false;
					}

					// Guideline 3.
					double point1Open = Data.Open[points[1].Bar];
					double point1Close = Data.Close[points[1].Bar];
					double wave1maxClosingRange = trendDirection > 0.0 ? Math.Max(point1Open, point1Close) : Math.Min(point1Open, point1Close);
					if (UtilityMethods.ComparePrices(wave1maxClosingRange, Data.Close[points[4].Bar], trendDirection) < 0.0)
					{
						isValidTrend = false;
					}

					// All the conditions are met up to the 5th wave, so we'll assume this next one is a 5th wave.
					if (isValidTrend)
					{
						fifthWaveDirection = trendDirection;
					}
				}

				// We're in a 5th wave, so lets label it to see on the chart. Also, the next point we'll label
				// as the 5th point and then save that we aren't in the 5th wave.
				if (fifthWaveDirection != 0.0)
				{
					// Label the 5th wave from the 4th point to now.
					int fourthWaveBar = points[4].Bar + 1;
					for (int j = fourthWaveBar; j <= currentBar; j++)
					{
						FifthWaveValue[j] = fifthWaveDirection > 0.0 ? Data.Low[j] : Data.High[j];
						FifthWaveDirection[j] = fifthWaveDirection;
					}

					// See if we can label the 5th point.
					List<double> zigzagSeries = fifthWaveDirection > 0.0 ? zigzag.ZigZagHighs : zigzag.ZigZagLows;
					if (zigzagSeries[searchBar] > 0.0)
					{
						labels[5] = new WavePointWithLabel() { Price = zigzag.Value[searchBar], Label = "5" };
					}

					// Label all the points regardless if we have a 5th point or not.
					for (int j = 0; j < 6; j++)
					{
						if (labels[j] != null)
						{
							WaveLabels[labels[j].Bar] = labels[j];
						}
					}

					// Reset searching for the next bar.
					fifthWaveDirection = 0.0;
				}
			}

		
		}


	}
}
