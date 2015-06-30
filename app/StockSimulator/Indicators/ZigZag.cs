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
	/// The ZigZag indicator shows trend lines filtering out changes below a defined level. 
	/// IMPORTANT!
	/// This indicator CANNOT be used for by signals unless you only use the values from
	/// 2 bars ago. This is because it lags by 2 bars but writes the data found in the past.
	/// </summary>
	public class ZigZag : Indicator
	{
		public List<double> Value { get; set; }
		public List<double> ZigZagHighs { get; set; }
		public List<double> ZigZagLows { get; set; }

		private enum DeviationType
		{
			Percent = 0,
			Points = 1,
		}

		private double currentZigZagHigh = 0;
		private double currentZigZagLow = 0;
		private DeviationType deviationType = DeviationType.Percent;
		private double deviationValue = 1.0;
		private List<double> zigZagHighSeries;
		private List<double> zigZagLowSeries;
		private int lastSwingIdx = -1;
		private double lastSwingPrice = 0.0;
		private int trendDir = 0; // 1 = trend up, -1 = trend down, init = 0
		private bool useHighLow = true;

		public ZigZag(TickerData tickerData, double devValue)
			: base(tickerData)
		{
			deviationValue = devValue;
		}

		/// <summary>
		/// Resets the indicator to it's starting state.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			currentZigZagHigh = 0;
			currentZigZagLow = 0;
			deviationType = DeviationType.Percent;
			lastSwingIdx = -1;
			lastSwingPrice = 0.0;
			trendDir = 0; // 1 = trend up, -1 = trend down, init = 0
			useHighLow = true;

			Value = Enumerable.Repeat(0d, Data.NumBars).ToList();
			ZigZagHighs = Enumerable.Repeat(0d, Data.NumBars).ToList();
			ZigZagLows = Enumerable.Repeat(0d, Data.NumBars).ToList();
			zigZagHighSeries = Enumerable.Repeat(0d, Data.NumBars).ToList();
			zigZagLowSeries = Enumerable.Repeat(0d, Data.NumBars).ToList();
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
			return "ZigZag," + deviationValue.ToString();
		}

		/// <summary>
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			// Add the indicator for plotting
			PlotSeries line = new PlotSeries("line");
			line.ShouldConnectNulls = true;
			ChartPlots[ToString()] = line;
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public override void AddToPlots(int currentBar)
		{
			base.AddToPlots(currentBar);

			PlotSeries line = (PlotSeries)ChartPlots[ToString()];

			long ticks = UtilityMethods.UnixTicks(Data.Dates[currentBar]);
			line.PlotData.Add(new List<object>()
			{
				ticks,
				Value[currentBar] > 0.0 ? (object)Math.Round(Value[currentBar], 2) : null
			});
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar < 2) // need 3 bars to calculate Low/High
			{
				return;
			}

			// Initialization
			if (lastSwingPrice == 0.0)
			{
				lastSwingPrice = Data.Close[currentBar];
			}

			List<double> highSeries = Data.High;
			List<double> lowSeries = Data.Low;

			if (!useHighLow)
			{
				highSeries = Data.Close;
				lowSeries = Data.Close;
			}

			// Calculation always for 1-bar ago!
			double tickSize = Data.TickSize;
			bool isSwingHigh = highSeries[currentBar - 1] >= highSeries[currentBar] - double.Epsilon
								&& highSeries[currentBar - 1] >= highSeries[currentBar - 2] - double.Epsilon;
			bool isSwingLow = lowSeries[currentBar - 1] <= lowSeries[currentBar] + double.Epsilon
								&& lowSeries[currentBar - 1] <= lowSeries[currentBar - 2] + double.Epsilon;
			bool isOverHighDeviation = (deviationType == DeviationType.Percent && IsPriceGreater(highSeries[currentBar - 1], (lastSwingPrice * (1.0 + deviationValue * 0.01))))
										|| (deviationType == DeviationType.Points && IsPriceGreater(highSeries[currentBar - 1], lastSwingPrice + deviationValue));
			bool isOverLowDeviation = (deviationType == DeviationType.Percent && IsPriceGreater(lastSwingPrice * (1.0 - deviationValue * 0.01), lowSeries[currentBar - 1]))
										|| (deviationType == DeviationType.Points && IsPriceGreater(lastSwingPrice - deviationValue, lowSeries[currentBar - 1]));

			double saveValue = 0.0;
			bool addHigh = false;
			bool addLow = false;
			bool updateHigh = false;
			bool updateLow = false;

			ZigZagHighs[currentBar] = 0;
			ZigZagLows[currentBar] = 0;

			if (!isSwingHigh && !isSwingLow)
			{
				zigZagHighSeries[currentBar] = currentZigZagHigh;
				zigZagLowSeries[currentBar] = currentZigZagLow;
				return;
			}

			if (trendDir <= 0 && isSwingHigh && isOverHighDeviation)
			{
				saveValue = highSeries[currentBar - 1];
				addHigh = true;
				trendDir = 1;
			}
			else if (trendDir >= 0 && isSwingLow && isOverLowDeviation)
			{
				saveValue = lowSeries[currentBar - 1];
				addLow = true;
				trendDir = -1;
			}
			else if (trendDir == 1 && isSwingHigh && IsPriceGreater(highSeries[currentBar - 1], lastSwingPrice))
			{
				saveValue = highSeries[currentBar - 1];
				updateHigh = true;
			}
			else if (trendDir == -1 && isSwingLow && IsPriceGreater(lastSwingPrice, lowSeries[currentBar - 1]))
			{
				saveValue = lowSeries[currentBar - 1];
				updateLow = true;
			}

			if (addHigh || addLow || updateHigh || updateLow)
			{
				if (updateHigh && lastSwingIdx >= 0)
				{
					int updateBar = currentBar - (currentBar - lastSwingIdx);
					ZigZagHighs[updateBar] = 0;
					Value[updateBar] = 0;
				}
				else if (updateLow && lastSwingIdx >= 0)
				{
					int updateBar = currentBar - (currentBar - lastSwingIdx);
					ZigZagLows[updateBar] = 0;
					Value[updateBar] = 0;
				}

				if (addHigh || updateHigh)
				{
					ZigZagHighs[currentBar - 1] = saveValue;
					ZigZagHighs[currentBar] = 0;

					currentZigZagHigh = saveValue;
					zigZagHighSeries[currentBar - 1] = currentZigZagHigh;
					Value[currentBar - 1] = currentZigZagHigh;
				}
				else if (addLow || updateLow)
				{
					ZigZagLows[currentBar - 1] = saveValue;
					ZigZagLows[currentBar] = 0;

					currentZigZagLow = saveValue;
					zigZagLowSeries[currentBar - 1] = currentZigZagLow;
					Value[currentBar - 1] = currentZigZagLow;
				}

				lastSwingIdx = currentBar - 1;
				lastSwingPrice = saveValue;
			}

			zigZagHighSeries[currentBar] = currentZigZagHigh;
			zigZagLowSeries[currentBar] = currentZigZagLow;
		}

		/// <summary>
		/// Returns if the price a is greater than b depending on the tick size
		/// </summary>
		/// <param name="a">First number</param>
		/// <param name="b">Second number</param>
		/// <returns>True if a > b</returns>
		private bool IsPriceGreater(double a, double b)
		{
			if (a > b && a - b > Data.TickSize / 2.0)
				return true;
			else
				return false;
		}

	}
}
