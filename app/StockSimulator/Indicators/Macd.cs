using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// Macd indicator
	/// </summary>
	class Macd : Indicator
	{
		public List<double> Value { get; set; }
		public List<double> Avg { get; set; }
		public List<double> Diff { get; set; }

		private List<double> _slowEma { get; set; }
		private List<double> _fastEma { get; set; }

		private int _fast = 12;
		private int _slow = 26;
		private int _smooth = 9;

		public Macd(TickerData tickerData, RunnableFactory factory) 
			: base(tickerData, factory)
		{
			Value = Enumerable.Repeat(0d, Data.NumBars).ToList();
			Avg = Enumerable.Repeat(0d, Data.NumBars).ToList();
			Diff = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_slowEma = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_fastEma = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Macd";
		}
	
		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the rsi for plotting
			PlotSeries plotMacd = new PlotSeries("line");
			PlotSeries plotAvg = new PlotSeries("line");
			PlotSeries plotDiff = new PlotSeries("column");
			ChartPlots["Macd"] = plotMacd;
			ChartPlots["Avg"] = plotAvg;
			ChartPlots["Diff"] = plotDiff;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long dateTicks = UtilityMethods.UnixTicks(Data.Dates[i]);
				plotMacd.PlotData.Add(new List<object>()
				{
					dateTicks,
					Math.Round(Value[i], 2)
				});
				plotAvg.PlotData.Add(new List<object>()
				{
					dateTicks,
					Math.Round(Avg[i], 2)
				});
				plotDiff.PlotData.Add(new List<object>()
				{
					dateTicks,
					Math.Round(Diff[i], 2)
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

			if (currentBar == 0)
			{
				_fastEma[currentBar] = Data.Close[currentBar];
				_slowEma[currentBar] = Data.Close[currentBar];
			}
			else
			{
				_fastEma[currentBar] = ((2.0 / (1 + _fast)) * Data.Close[currentBar] + (1 - (2.0 / (1 + _fast))) * _fastEma[currentBar - 1]);
				_slowEma[currentBar] = ((2.0 / (1 + _slow)) * Data.Close[currentBar] + (1 - (2.0 / (1 + _slow))) * _slowEma[currentBar - 1]);

				double macd = _fastEma[currentBar] - _slowEma[currentBar];
				double macdAvg = (2.0 / (1 + _smooth)) * macd + (1 - (2.0 / (1 + _smooth))) * Avg[currentBar - 1];

				Value[currentBar] = macd;
				Avg[currentBar] = macdAvg;
				Diff[currentBar] = macd - macdAvg;
			}
		}

	}
}
