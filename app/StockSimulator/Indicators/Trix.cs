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
	/// Trix
	/// </summary>
	class Trix : Indicator
	{
		public List<double> Default { get; set; }
		public List<double> Signal { get; set; }

		private List<double> _firstEma { get; set; }
		private List<double> _secondEma { get; set; }
		private List<double> _tripleEma { get; set; }

		private int _period = 14;
		private int _signalPeriod = 3;

		public Trix(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			Default = Enumerable.Repeat(0d, Data.NumBars).ToList();
			Signal = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_firstEma = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_secondEma = Enumerable.Repeat(0d, Data.NumBars).ToList();
			_tripleEma = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Trix";
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the indicator for plotting
			PlotSeries plotDefault = new PlotSeries("line");
			PlotSeries plotSignal = new PlotSeries("line");
			ChartPlots[ToString() + " Default"] = plotDefault;
			ChartPlots[ToString() + " Signal"] = plotSignal;
			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long ticks = UtilityMethods.UnixTicks(Data.Dates[i]);
				plotDefault.PlotData.Add(new List<object>()
				{
					ticks,
					Math.Round(Default[i], 6)
				});
				plotSignal.PlotData.Add(new List<object>()
				{
					ticks,
					Math.Round(Signal[i], 6)
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
				Default[currentBar] = Data.Close[currentBar];
				Signal[currentBar] = Data.Close[currentBar];
				_firstEma[currentBar] = Data.Close[currentBar];
				_secondEma[currentBar] = Data.Close[currentBar];
				_tripleEma[currentBar] = Data.Close[currentBar];
				return;
			}

			_firstEma[currentBar] = UtilityMethods.Ema(Data.Close, _firstEma, currentBar, _period);
			_secondEma[currentBar] = UtilityMethods.Ema(_firstEma, _secondEma, currentBar, _period);
			_tripleEma[currentBar] = UtilityMethods.Ema(_secondEma, _tripleEma, currentBar, _period);

			double trix = 100 * ((_tripleEma[currentBar] - _tripleEma[currentBar - 1]) / _tripleEma[currentBar]);

			Default[currentBar] = trix;
			Signal[currentBar] = UtilityMethods.Ema(Default, Signal, currentBar, _signalPeriod);
		}
	}
}
