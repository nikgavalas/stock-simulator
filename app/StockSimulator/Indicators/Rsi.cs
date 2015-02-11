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
	/// Rsi indicator
	/// </summary>
	class Rsi : Indicator
	{
		public List<double> Value { get; set; }

		private List<double> _up { get; set; }
		private List<double> _down { get; set; }
		private List<double> _avgUp { get; set; }
		private List<double> _avgDown { get; set; }

		private int _period = 14;

		public Rsi(TickerData tickerData, RunnableFactory factory, int period) 
			: base(tickerData, factory)
		{
			Value = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_up = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_down = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_avgUp = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_avgDown = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_period = period;
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Rsi" + _period.ToString();
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the rsi for plotting
			PlotSeries plot = new PlotSeries("line");
			ChartPlots["Rsi"] = plot;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				plot.PlotData.Add(new List<object>()
				{
					ExtensionMethods.UnixTicks(Data.Dates[i]),
					Math.Round(Value[i], 2)
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

			if (currentBar - 1 < _period)
			{
				return;
			}

			_down[currentBar] = Math.Max(Data.Close[currentBar - 1] - Data.Close[currentBar], 0);
			_up[currentBar] = Math.Max(Data.Close[currentBar] - Data.Close[currentBar - 1], 0);

			if ((currentBar + 1) == _period)
			{
				// First averages 
				_avgDown[currentBar] = _down.Average();
				_avgUp[currentBar] = _up.Average();
			}
			else
			{
				// Rest of averages are smoothed
				_avgDown[currentBar] = (_avgDown[currentBar - 1] * (_period - 1) + _down[currentBar]) / _period;
				_avgUp[currentBar] = (_avgUp[currentBar - 1] * (_period - 1) + _up[currentBar]) / _period;
			}

			Value[currentBar] = _avgDown[currentBar] == 0 ? 100 : 100 - 100 / (1 + _avgUp[currentBar] / _avgDown[currentBar]);
		}
	}
}
