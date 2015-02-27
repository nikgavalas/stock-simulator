using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core;

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

		public Trend(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			UpTrend = Enumerable.Repeat(false, Data.NumBars).ToList();
			DownTrend = Enumerable.Repeat(false, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"Swing"
				};

				return deps;
			}
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
			PlotSeries plotUp = new PlotSeries("column");
			PlotSeries plotDown = new PlotSeries("column");
			ChartPlots[ToString() + "Up"] = plotUp;
			ChartPlots[ToString() + "Down"] = plotDown;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long ticks = ExtensionMethods.UnixTicks(Data.Dates[i]);
				plotUp.PlotData.Add(new List<object>()
				{
					ticks,
					UpTrend[i] ? 1 : 0
				});
				plotDown.PlotData.Add(new List<object>()
				{
					ticks,
					DownTrend[i] ? -1 : 0
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

			//CalculateTrendLines(currentBar);
		}

		/// <summary>
		/// Calculate trend lines and prevailing trend
		/// <param name="currentBar">Current bar of the simulation</param>
		/// </summary>
		private void CalculateTrendLines(int currentBar)
		{
			// Calculate up trend line
			int upTrendStartBarsAgo = 0;
			int upTrendEndBarsAgo = 0;
			int upTrendOccurence = 1;

			Swing swing = (Swing)Dependents[0];

			while (Data.Low[currentBar - upTrendEndBarsAgo] <= Data.Low[currentBar - upTrendStartBarsAgo])
			{
				upTrendStartBarsAgo = swing.SwingLowBar(currentBar, 0, upTrendOccurence + 1, currentBar);
				upTrendEndBarsAgo = swing.SwingLowBar(currentBar, 0, upTrendOccurence, currentBar);

				if (upTrendStartBarsAgo < 0 || upTrendEndBarsAgo < 0)
				{
					break;
				}

				upTrendOccurence++;
			}


			// Calculate down trend line	
			int downTrendStartBarsAgo = 0;
			int downTrendEndBarsAgo = 0;
			int downTrendOccurence = 1;

			while (Data.High[currentBar - downTrendEndBarsAgo] >= Data.High[currentBar - downTrendStartBarsAgo])
			{
				downTrendStartBarsAgo = swing.SwingHighBar(currentBar, 0, downTrendOccurence + 1, currentBar);
				downTrendEndBarsAgo = swing.SwingHighBar(currentBar, 0, downTrendOccurence, currentBar);

				if (downTrendStartBarsAgo < 0 || downTrendEndBarsAgo < 0)
				{
					break;
				}

				downTrendOccurence++;
			}

			if (upTrendStartBarsAgo > 0 && upTrendEndBarsAgo > 0 && upTrendStartBarsAgo < downTrendStartBarsAgo)
			{
				UpTrend[currentBar] = true;
				DownTrend[currentBar] = false;
			}
			else if (downTrendStartBarsAgo > 0 && downTrendEndBarsAgo > 0 && upTrendStartBarsAgo > downTrendStartBarsAgo)
			{
				UpTrend[currentBar] = false;
				DownTrend[currentBar] = true;
			}
			else
			{
				UpTrend[currentBar] = false;
				DownTrend[currentBar] = false;
			}
		}
	}
}
