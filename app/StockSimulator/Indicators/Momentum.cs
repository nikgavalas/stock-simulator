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
	/// Momentum indicator
	/// </summary>
	class Momentum : Indicator
	{
		public List<double> Value { get; set; }

		private int _period = 14;

		public Momentum(TickerData tickerData, RunnableFactory factory, int period) 
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
			return "Momentum" + _period.ToString();
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the rsi for plotting
			PlotSeries plot = new PlotSeries("line");
			ChartPlots["Momentum"] = plot;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				plot.PlotData.Add(new List<object>()
				{
					UtilityMethods.UnixTicks(Data.Dates[i]),
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
				Value[currentBar] = Data.Close[currentBar] - Data.Close[currentBar - Math.Min(currentBar, _period)];
			}
		}
	}
}
