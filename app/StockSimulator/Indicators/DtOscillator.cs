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
	/// DT Oscillator used in Robert Miner's book. While his formula isn't exactly spelled
	/// out in his book there are some online approximations.
	/// http://www.wisestocktrader.com/indicators/3494-dtosc
	/// </summary>
	public class DtOscillator : Indicator
	{
		public List<double> SD { get; set; }
		public List<double> SK { get; set; }
		public List<double> StoRsi { get; set; }

		private int _periodRsi = 13;
		private int _periodStoch = 8;
		private int _periodSK = 5;
		private int _periodSD = 5;

		public DtOscillator(TickerData tickerData, RunnableFactory factory, string[] settings)
			: base(tickerData, factory)
		{
			_periodRsi = Convert.ToInt32(settings[0]);
			_periodStoch = Convert.ToInt32(settings[1]);
			_periodSK = Convert.ToInt32(settings[2]);
			_periodSD = Convert.ToInt32(settings[3]);

			SD = Enumerable.Repeat(0d, Data.NumBars).ToList();
			SK = Enumerable.Repeat(0d, Data.NumBars).ToList();
			StoRsi = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"Rsi," + _periodRsi.ToString()
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
			return "DtOscillator," + _periodRsi.ToString() + "," + _periodStoch.ToString() + "," + _periodSK.ToString() + "," + _periodSD.ToString();
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the indicator for plotting
			PlotSeries plotD = new PlotSeries("line");
			PlotSeries plotK = new PlotSeries("line");
			ChartPlots["DtOscillator %D"] = plotD;
			ChartPlots["DtOscillator %K"] = plotK;

			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long ticks = UtilityMethods.UnixTicks(Data.Dates[i]);
				plotD.PlotData.Add(new List<object>()
				{
					ticks,
					Math.Round(SD[i], 2)
				});
				plotK.PlotData.Add(new List<object>()
				{
					ticks,
					Math.Round(SK[i], 2)
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

			Rsi rsi = (Rsi)Dependents[0];

			double rsiMinValue = UtilityMethods.Min(rsi.Value, currentBar, _periodStoch);
			double rsiMaxValue = UtilityMethods.Max(rsi.Value, currentBar, _periodStoch);
			double denom = rsiMaxValue - rsiMinValue;

			StoRsi[currentBar] = denom > 0.0 ? 100.0 * ((rsi.Value[currentBar] - rsiMinValue) / denom) : 0.0;

			// SMA or EMA depending on preference.
			SK[currentBar] = UtilityMethods.Sma(StoRsi, currentBar, _periodSK);
			SD[currentBar] = UtilityMethods.Sma(SK, currentBar, _periodSD);
			//SK[currentBar] = UtilityMethods.Ema(StoRsi, SK, currentBar, _periodSK);
			//SD[currentBar] = UtilityMethods.Ema(SK, SD, currentBar, _periodSK);

		}
	}
}
