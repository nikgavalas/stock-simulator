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

		#region Configurables
		public int PeriodRsi
		{
			get { return _periodRsi; }
			set { _periodRsi = value; }
		}

		public int PeriodStoch
		{
			get { return _periodStoch; }
			set { _periodStoch = value; }
		}

		public int PeriodSK
		{
			get { return _periodSK; }
			set { _periodSK = value; }
		}

		public int PeriodSD
		{
			get { return _periodSD; }
			set { _periodSD = value; }
		}
		
		private int _periodRsi = 13;
		private int _periodStoch = 8;
		private int _periodSK = 5;
		private int _periodSD = 5;
		#endregion

		/// <summary>
		/// Creates the indicator.
		/// Add any dependents here.
		/// </summary>
		/// <param name="tickerData">Price data</param>
		public DtOscillator(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				(Runnable)new Rsi(Data) { Period = PeriodRsi }
			};
		}

		/// <summary>
		/// Resets the indicator to it's starting state.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			SD = Enumerable.Repeat(0d, Data.NumBars).ToList();
			SK = Enumerable.Repeat(0d, Data.NumBars).ToList();
			StoRsi = Enumerable.Repeat(0d, Data.NumBars).ToList();
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
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			// Add the indicator for plotting
			PlotSeries plotD = new PlotSeries("line");
			PlotSeries plotK = new PlotSeries("line");
			ChartPlots["DtOscillator %D"] = plotD;
			ChartPlots["DtOscillator %K"] = plotK;
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public override void AddToPlots(int currentBar)
		{
			base.AddToPlots(currentBar);

			PlotSeries plotD = (PlotSeries)ChartPlots["DtOscillator %D"];
			PlotSeries plotK = (PlotSeries)ChartPlots["DtOscillator %K"];

			long ticks = UtilityMethods.UnixTicks(Data.Dates[currentBar]);
			plotD.PlotData.Add(new List<object>()
			{
				ticks,
				Math.Round(SD[currentBar], 2)
			});
			plotK.PlotData.Add(new List<object>()
			{
				ticks,
				Math.Round(SK[currentBar], 2)
			});
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			Rsi rsi = (Rsi)_dependents[0];

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
