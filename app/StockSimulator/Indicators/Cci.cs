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
	/// CCI indicator
	/// </summary>
	class Cci : Indicator
	{
		public List<double> Value { get; set; }

		private int _period = 14;

		public Cci(TickerData tickerData, RunnableFactory factory, int period)
			: base(tickerData, factory)
		{
			Value = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_period = period;
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Cci" + _period.ToString();
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the rsi for plotting
			PlotSeries plot = new PlotSeries("line");
			ChartPlots["Cci"] = plot;
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

			if (currentBar > 0)
			{
				double mean = 0;
				int avgStartIndex = currentBar - Math.Min(currentBar, _period - 1);
				double avg = Data.Typical.GetRange(avgStartIndex, Math.Min(currentBar, _period)).Average();
				for (int idx = Math.Min(currentBar, _period - 1); idx >= 0; idx--)
				{
					mean += Math.Abs(Data.Typical[currentBar - idx] - avg);
				}
				
				Value[currentBar] = (Data.Typical[currentBar] - avg) / (mean == 0 ? 1 : (0.015 * (mean / Math.Min(_period, currentBar + 1))));
			}

		}
	}
}
