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
	public class StochRsi : Indicator
	{
		public List<double> Value { get; set; }

		private int _period = 11;

		public StochRsi(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			Value = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "StochRsi";
		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"Rsi,11"
				};

				return deps;
			}
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the rsi for plotting
			PlotSeries plotValue = new PlotSeries("line");
			ChartPlots[ToString()] = plotValue;

			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long ticks = UtilityMethods.UnixTicks(Data.Dates[i]);
				plotValue.PlotData.Add(new List<object>()
				{
					ticks,
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

			Rsi rsiInd = (Rsi)Dependents[0];
			double rsi = rsiInd.Value[currentBar];
			double rsiL = UtilityMethods.Min(rsiInd.Value, currentBar, _period);
			double rsiH = UtilityMethods.Max(rsiInd.Value, currentBar, _period);

			if (rsi != rsiL && rsiH != rsiL)
			{
				Value[currentBar] = (rsi - rsiL) / (rsiH - rsiL);
			}
			else
			{
				Value[currentBar] = 0;
			}
		}
	}
}
