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
	/// Dmi
	/// </summary>
	class Dmi : Indicator
	{
		public List<double> Value { get; set; }

		private int period = 9;
		private List<double> dmMinus;
		private List<double> dmPlus;
		private List<double> tr;

		public Dmi(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			Value = Enumerable.Repeat(0d, Data.NumBars).ToList();
			dmMinus = Enumerable.Repeat(0d, Data.NumBars).ToList();
			dmPlus = Enumerable.Repeat(0d, Data.NumBars).ToList();
			tr = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Dmi";
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the rsi for plotting
			PlotSeries plot = new PlotSeries("line");
			ChartPlots[ToString()] = plot;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				plot.PlotData.Add(new List<object>()
				{
					UtilityMethods.UnixTicks(Data.Dates[i]),
					Math.Round(Value[i], 6)
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
				dmMinus[currentBar] = 0;
				dmPlus[currentBar] = 0;
				tr[currentBar] = Data.High[currentBar] - Data.Low[currentBar];
				Value[currentBar] = 0;
			}
			else
			{
				dmMinus[currentBar] = Data.Low[currentBar - 1] - Data.Low[currentBar] > Data.High[currentBar] - Data.High[currentBar - 1] ? Math.Max(Data.Low[currentBar - 1] - Data.Low[currentBar], 0) : 0;
				dmPlus[currentBar] = Data.High[currentBar] - Data.High[currentBar - 1] > Data.Low[currentBar - 1] - Data.Low[currentBar] ? Math.Max(Data.High[currentBar] - Data.High[currentBar - 1], 0) : 0;
				tr[currentBar] = Math.Max(Data.High[currentBar] - Data.Low[currentBar], Math.Max(Math.Abs(Data.High[currentBar] - Data.Close[currentBar - 1]), Math.Abs(Data.Low[currentBar] - Data.Close[currentBar - 1])));

				double diPlus = UtilityMethods.Sma(tr, currentBar, period) == 0 ? 0 : UtilityMethods.Sma(dmPlus, currentBar, period) / UtilityMethods.Sma(tr, currentBar, period);
				double diMinus = UtilityMethods.Sma(tr, currentBar, period) == 0 ? 0 : UtilityMethods.Sma(dmMinus, currentBar, period) / UtilityMethods.Sma(tr, currentBar, period);

				Value[currentBar] = (diPlus + diMinus == 0) ? 0 : (diPlus - diMinus) / (diPlus + diMinus);
			}
		}
	}
}
