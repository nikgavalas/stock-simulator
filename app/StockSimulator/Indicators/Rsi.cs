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
		public List<double> Avg { get; set; }

		private List<double> _up { get; set; }
		private List<double> _down { get; set; }
		private List<double> _avgUp { get; set; }
		private List<double> _avgDown { get; set; }

		#region Configurables
		public int Period
		{
			get { return _period; }
			set { _period = value; }
		}

		public int Smooth
		{
			get { return _smooth; }
			set { _smooth = value; }
		}

		private int _period = 14;
		private int _smooth = 3;
		#endregion

		/// <summary>
		/// Creates the indicator.
		/// Add any dependents here.
		/// </summary>
		/// <param name="tickerData">Price data</param>
		public Rsi(TickerData tickerData) 
			: base(tickerData)
		{
			Value = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			Avg = UtilityMethods.CreateList<double>(Data.NumBars, 0d);

			_up = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			_down = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			_avgUp = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			_avgDown = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
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
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			// Add the indicator for plotting
			PlotSeries line = new PlotSeries("line");
			ChartPlots[ToString()] = line;
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public override void AddToPlots(int currentBar)
		{
			base.AddToPlots(currentBar);

			PlotSeries line = (PlotSeries)ChartPlots[ToString()];
			line.PlotData.Add(new List<object>()
			{
				UtilityMethods.UnixTicks(Data.Dates[currentBar]),
				Math.Round(Value[currentBar], 2)
			});
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar == 0)
			{
				if (_period < 3)
				{
					Avg[currentBar] = 50.0;
				}

				return;
			}

			_down[currentBar] = Math.Max(Data.Close[currentBar - 1] - Data.Close[currentBar], 0);
			_up[currentBar] = Math.Max(Data.Close[currentBar] - Data.Close[currentBar - 1], 0);

			if (currentBar + 1 < _period)
			{
				if ((currentBar + 1) == (_period - 1))
				{
					Avg[currentBar] = 50.0;
				}

				return;
			}

			if ((currentBar + 1) == _period)
			{
				// First averages 
				_avgDown[currentBar] = _down.GetRange(0, _period).Average();
				_avgUp[currentBar] = _up.GetRange(0, _period).Average();
			}
			else
			{
				// Rest of averages are smoothed
				_avgDown[currentBar] = (_avgDown[currentBar - 1] * (_period - 1) + _down[currentBar]) / _period;
				_avgUp[currentBar] = (_avgUp[currentBar - 1] * (_period - 1) + _up[currentBar]) / _period;
			}
			double rsi = _avgDown[currentBar] == 0 ? 100 : 100 - 100 / (1 + _avgUp[currentBar] / _avgDown[currentBar]);
			double rsiAvg = (2.0 / (1 + _smooth)) * rsi + (1 - (2.0 / (1 + _smooth))) * Avg[currentBar - 1];

			Value[currentBar] = rsi;
			Avg[currentBar] = rsiAvg;
		}
	}
}
