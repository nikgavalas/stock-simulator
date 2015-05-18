using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core;
using Newtonsoft.Json;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// Base candlestick class which holds things that all candlesticks need.
	/// This class is meant to be derived.
	/// </summary>
	class Trend : Indicator
	{
		public List<bool> UpTrend { get; set; }
		public List<bool> DownTrend { get; set; }
		public List<double> UpTrendPlot { get; set; }
		public List<double> DownTrendPlot { get; set; }

		public Trend(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			UpTrend = Enumerable.Repeat(false, Data.NumBars).ToList();
			DownTrend = Enumerable.Repeat(false, Data.NumBars).ToList();
			UpTrendPlot = Enumerable.Repeat(0d, Data.NumBars).ToList();
			DownTrendPlot = Enumerable.Repeat(0d, Data.NumBars).ToList();
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
			return "Trend";
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the rsi for plotting
			PlotSeries plotUp = new PlotSeries("line");
			PlotSeries plotDown = new PlotSeries("line");
			ChartPlots[ToString() + "Up"] = plotUp;
			ChartPlots[ToString() + "Down"] = plotDown;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long ticks = UtilityMethods.UnixTicks(Data.Dates[i]);

				object upValue = null;
				if (UpTrendPlot[i] > 0.0)
				{
					upValue = UpTrendPlot[i];
				}
				plotUp.PlotData.Add(new List<object>()
				{
					ticks,
					upValue
				});

				object downValue = null;
				if (DownTrendPlot[i] > 0.0)
				{
					downValue = DownTrendPlot[i];
				}
				plotDown.PlotData.Add(new List<object>()
				{
					ticks,
					downValue
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

			if (currentBar < Simulator.Config.TrendStrength)
			{
				return;
			}

			// An uptrend is a series of bars with higher and higher lows.
			bool isUptrend = true;
			for (int i = currentBar; i > currentBar - Simulator.Config.TrendStrength; i--)
			{
				if (Data.Low[i] < Data.Low[i - 1])
				{
					isUptrend = false;
					break;
				}
			}
			UpTrend[currentBar] = isUptrend;

			// Plot the downtrend at the top of the prices.
			if (isUptrend == true)
			{
				for (int i = 0; i < Simulator.Config.TrendStrength; i++)
				{
					int index = currentBar - i;
					UpTrendPlot[index] = Data.Low[index];
				}
			}

			// A downtrend is a series of bars with lower and lower highs.
			bool isDowntrend = true;
			for (int i = currentBar; i > currentBar - Simulator.Config.TrendStrength; i--)
			{
				if (Data.High[i] > Data.High[i - 1])
				{
					isDowntrend = false;
					break;
				}
			}
			DownTrend[currentBar] = isDowntrend;

			// Plot the downtrend at the top of the prices.
			if (isDowntrend == true)
			{
				for (int i = 0; i < Simulator.Config.TrendStrength; i++)
				{
					int index = currentBar - i;
					DownTrendPlot[index] = Data.High[index];
				}
			}
		}

	}
}
