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
	/// StochasticsFast
	/// </summary>
	class StochasticsFast : Indicator
	{
		public List<double> D { get; set; }
		public List<double> K { get; set; }

		private List<double> _den { get; set; }
		private List<double> _nom { get; set; }

		private int _periodD = 3;
		private int _periodK = 14;

		public StochasticsFast(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			D = Enumerable.Repeat(0d, Data.NumBars).ToList();
			K = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_den = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_nom = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "StochasticsFast";
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the rsi for plotting
			PlotSeries plotD = new PlotSeries("line");
			PlotSeries plotK = new PlotSeries("line");
			ChartPlots[ToString() + " %D"] = plotD;
			ChartPlots[ToString() + " %K"] = plotK;
			
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long ticks = UtilityMethods.UnixTicks(Data.Dates[i]);
				plotD.PlotData.Add(new List<object>()
				{
					ticks,
					Math.Round(D[i], 2)
				});
				plotK.PlotData.Add(new List<object>()
				{
					ticks,
					Math.Round(K[i], 2)
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

			double minLowK = UtilityMethods.Min(Data.Low, currentBar, _periodK);
			_nom[currentBar] = Data.Close[currentBar] - minLowK;
			_den[currentBar] = UtilityMethods.Max(Data.High, currentBar, _periodK) - minLowK;

			if (_den[currentBar] < 0.000000000001 && _den[currentBar] >= 0.0)
			{
				K[currentBar] = currentBar == 0 ? 50 : K[currentBar - 1];
			}
			else
			{
				K[currentBar] = Math.Min(100, Math.Max(0, 100 * _nom[currentBar] / _den[currentBar]));
			}

			D[currentBar] = UtilityMethods.Sma(K, currentBar, _periodD);
		}
	}
}
