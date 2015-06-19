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
	/// From Bresserts article. Find the average time between cycle lows
	/// and highs. Then padd it left and right and see if this bar is
	/// on one of those cycles.
	/// </summary>
	class BressertTimingBands : Indicator
	{
		public List<bool> HighCycle { get; set; }
		public List<int> HighCycleAvg { get; set; }
		public List<bool> LowCycle { get; set; }
		public List<int> LowCycleAvg { get; set; }
		public List<double> HighCyclePlot { get; set; }
		public List<double> LowCyclePlot { get; set; }

		public BressertTimingBands(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			HighCycle = Enumerable.Repeat(false, Data.NumBars).ToList();
			HighCycleAvg = Enumerable.Repeat(0, Data.NumBars).ToList();
			LowCycle = Enumerable.Repeat(false, Data.NumBars).ToList();
			LowCycleAvg = Enumerable.Repeat(0, Data.NumBars).ToList();
			HighCyclePlot = Enumerable.Repeat(0d, Data.NumBars).ToList();
			LowCyclePlot = Enumerable.Repeat(0d, Data.NumBars).ToList();
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
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"BressertDss"
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
			return "BressertTimingBands";
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
			ChartPlots[ToString() + "High"] = plotUp;
			ChartPlots[ToString() + "Low"] = plotDown;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long ticks = UtilityMethods.UnixTicks(Data.Dates[i]);

				object upValue = null;
				if (HighCyclePlot[i] > 0.0)
				{
					upValue = HighCyclePlot[i];
				}
				plotUp.PlotData.Add(new List<object>()
				{
					ticks,
					upValue
				});

				object downValue = null;
				if (LowCyclePlot[i] > 0.0)
				{
					downValue = LowCyclePlot[i];
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

			if (currentBar < Simulator.Config.BressertCycleLookback)
			{
				return;
			}

			BressertDss ind = (BressertDss)Dependents[0];

			List<int> valleyBars = new List<int>();
			List<int> peakBars = new List<int>();

			// Loop back through the preceding bars and record the number of bars 
			// between the cycle highs and lows.
			// Plus 2 to account for the IsValley and IsPeak methods.
			int lookbackEndBar = (currentBar - Simulator.Config.BressertCycleLookback) + 2;
			for (int i = currentBar; i > lookbackEndBar; i--)
			{
				if (DataSeries.IsBelow(ind.Value, 40, i, 0) != -1 && UtilityMethods.IsValley(ind.Value, i))
				{
					valleyBars.Add(i);
				}

				if (DataSeries.IsAbove(ind.Value, 50, i, 0) != -1 && UtilityMethods.IsPeak(ind.Value, i))
				{
					peakBars.Add(i);
				}
			}


			if (valleyBars.Count > 0)
			{
				int valleyAvg = CalculateAvgBarDifference(valleyBars);
				for (int i = 0; i < valleyBars.Count; i++)
				{
					int valleyTarget = valleyBars[i] + valleyAvg;

					// If this bar is the average distance from the last peak/valley +/- the padding then its
					// part of the timing band.
					for (int j = valleyTarget - Simulator.Config.BressertBandPadding; j <= valleyTarget + Simulator.Config.BressertBandPadding; j++)
					{
						if (j < Data.NumBars)
						{
							LowCycle[j] = true;
							LowCyclePlot[j] = Data.Low[j];
							LowCycleAvg[j] = valleyAvg;
						}
					}
				}
			}

			if (peakBars.Count > 0)
			{
				int peakAvg = CalculateAvgBarDifference(peakBars);
				for (int i = 0; i < peakBars.Count; i++)
				{
					int peakTarget = peakBars[i] + peakAvg;

					// If this bar is the average distance from the last peak/valley +/- the padding then its
					// part of the timing band.
					for (int j = peakTarget - Simulator.Config.BressertBandPadding; j <= peakTarget + Simulator.Config.BressertBandPadding; j++)
					{
						if (j < Data.NumBars)
						{
							HighCycle[j] = true;
							HighCyclePlot[j] = Data.High[j];
							HighCycleAvg[j] = peakAvg;
						}
					}
				}
			}

			//if (peakBars.Count > 0)
			//{
			//	int peakAvg = CalculateAvgBarDifference(peakBars);
			//	int peakTarget = peakBars[1] + peakAvg;
			//	if (currentBar >= peakTarget - Simulator.Config.BressertBandPadding && currentBar <= peakTarget + Simulator.Config.BressertBandPadding)
			//	{
			//		HighCycle[currentBar] = true;
			//		HighCyclePlot[currentBar] = Data.High[currentBar];
			//	}
			
			//	HighCycleAvg[currentBar] = peakAvg;
			//}

		}

		/// <summary>
		/// Calculates the average different between the bars indicies in the list.
		/// </summary>
		/// <param name="bars">List of bar indicies</param>
		/// <returns>Average difference between the bar indicies</returns>
		private int CalculateAvgBarDifference(List<int> bars)
		{
			// Override the auto mode with a hard coded value.
			//if (Simulator.Config.BressertBarsBetweenCycles > 0)
			{
				return Simulator.Config.BressertBarsBetweenCycles;
			}

			// TODO: this needs to be updated for the forward predicting bands.
			//if (bars.Count == 1)
			//{
			//	return bars[0];
			//}

			//List<int> diffs = new List<int>();
			//for (int i = bars.Count - 1; i > 0; i--)
			//{
			//	diffs.Add(bars[i - 1] - bars[i]);
			//}

			//return Convert.ToInt32(diffs.Average());
		}
	}
}
