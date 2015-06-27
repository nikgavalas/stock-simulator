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
		public class WavePointLabel
		{
			public double Price { get; set; }
			public string Label { get; set; }
			public int Bar { get; set; }
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
		public List<WavePointLabel> WaveLabels { get; set; }

		private readonly int _maxBarsForWave = 500;

		private double _5thWaveDirection = 0.0; // 1 is bull, -1 bear, 0 not in a 5th wave

		/// <summary>
		/// Creates the indicator.
		/// </summary>
		/// <param name="tickerData">Price data</param>
		public ElliotWaves(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				(Runnable)new ZigZag(Data, 5.0)
			};
		}

		/// <summary>
		/// Resets the indicator to it's starting state.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			FifthWaveValue = Enumerable.Repeat(0d, Data.NumBars).ToList();
			FifthWaveDirection = Enumerable.Repeat(0d, Data.NumBars).ToList();
			WaveLabels = Enumerable.Repeat<WavePointLabel>(null, Data.NumBars).ToList();
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
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the value for plotting
			PlotSeries plot = new PlotSeries("line");
			PlotSeriesFlag plotFlag = new PlotSeriesFlag();
			ChartPlots[ToString()] = plot;
			ChartPlots[ToString() + "label"] = plotFlag;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long ticks = UtilityMethods.UnixTicks(Data.Dates[i]);
				plot.PlotData.Add(new List<object>()
				{
					ticks,
					FifthWaveValue[i] > 0.0 ? (object)Math.Round(FifthWaveValue[i], 2) : null
				});

				if (WaveLabels[i] != null)
				{
					plotFlag.PlotData.Add(new Dictionary<string, object>()
					{
						{ "x", ticks },
						{ "title", WaveLabels[i].Label } 
					});
				}
			}
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar > 1)
			{
				ZigZag zigzag = (ZigZag)Dependents[0];

				int cutoffBar = Math.Max(0, currentBar - _maxBarsForWave);
				int searchBar = currentBar - 2;

				// See if we've just recently had the end of the 4th wave. For bull
				// waves this will start with a low and for bear will start with a high.
				// Need to check the past 3rd bar since that is the lag of the zigzag indicator.
				// For every zig there is a zag meaning if we found a low, the previous point will
				// be a high. This makes it easier to search back for the patterns.
				WavePoint[] points = new WavePoint[6];
				WavePointLabel[] labels = new WavePointLabel[6];
				List<double> currentSeries = null;
				int waveEndpointBeingSearchFor = 4;
				double trendDirection = 0.0;
				if (zigzag.ZigZagLows[searchBar] > 0.0)
				{
					trendDirection = Order.OrderType.Long;
					points[waveEndpointBeingSearchFor] = new WavePoint() { Bar = searchBar, Price = zigzag.Value[searchBar] };
					labels[waveEndpointBeingSearchFor] = new WavePointLabel() { Bar = searchBar, Price = zigzag.Value[searchBar], Label = waveEndpointBeingSearchFor.ToString() };
					currentSeries = zigzag.ZigZagHighs;
					--waveEndpointBeingSearchFor;
				}
				else if (zigzag.ZigZagHighs[searchBar] > 0.0)
				{
					trendDirection = Order.OrderType.Short;
					points[waveEndpointBeingSearchFor] = new WavePoint() { Bar = searchBar, Price = zigzag.Value[searchBar] };
					labels[waveEndpointBeingSearchFor] = new WavePointLabel() { Bar = searchBar, Price = zigzag.Value[searchBar], Label = waveEndpointBeingSearchFor.ToString() };
					currentSeries = zigzag.ZigZagLows;
					--waveEndpointBeingSearchFor;
				}

				// We have an endpoint on the 4th wave, so lets check and see if we
				// have all the previous waves by searching for the endpoints.
				if (currentSeries != null)
				{
					for (int i = searchBar - 1; i >= cutoffBar && waveEndpointBeingSearchFor >= 0; i--)
					{
						// Each time we find a wave endpoint, save the price and move to the next one for searching.
						if (currentSeries[i] > 0.0)
						{
							points[waveEndpointBeingSearchFor] = new WavePoint() { Bar = i, Price = zigzag.Value[i] };
							labels[waveEndpointBeingSearchFor] = new WavePointLabel() { Bar = i, Price = zigzag.Value[i], Label = waveEndpointBeingSearchFor.ToString() };
							currentSeries = currentSeries == zigzag.ZigZagHighs ? zigzag.ZigZagLows : zigzag.ZigZagHighs;
							--waveEndpointBeingSearchFor;
						}
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
						_5thWaveDirection = trendDirection;
					}
				}

				// We're in a 5th wave, so lets label it to see on the chart. Also, the next point we'll label
				// as the 5th point and then save that we aren't in the 5th wave.
				if (_5thWaveDirection != 0.0)
				{
					FifthWaveValue[currentBar] = _5thWaveDirection > 0.0 ? Data.Low[currentBar] : Data.High[currentBar];
					FifthWaveDirection[currentBar] = _5thWaveDirection;

					// Found the last point so lets end this wave.
					List<double> zigzagSeries = _5thWaveDirection > 0.0 ? zigzag.ZigZagHighs : zigzag.ZigZagLows;
					if (zigzagSeries[searchBar] > 0.0)
					{
						_5thWaveDirection = 0.0;
						labels[5] = new WavePointLabel() { Price = zigzag.Value[searchBar], Label = "5" };

						for (int j = 0; j < 6; j++)
						{
							WaveLabels[labels[j].Bar] = labels[j];
						}
					}
				}
			}

		
		}


	}
}
