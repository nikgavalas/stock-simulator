using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;
using Newtonsoft.Json;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// Sma indicator
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	class Sma : Indicator
	{
		public List<double> Avg { get; set; }

		#region Configurables
		public int Period
		{
			get { return _period; }
			set { _period = value; }
		}

		private int _period = 14;
		#endregion

		/// <summary>
		/// This indicator is plotted on the price bars.
		/// </summary>
		[JsonProperty("plotOnPrice")]
		public override bool PlotOnPrice
		{
			get { return true; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="tickerData">Ticker data for the indicator</param>
		public Sma(TickerData tickerData) 
			: base(tickerData)
		{
			Avg = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Sma" + Period.ToString();
		}

		/// <summary>
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			// Add the indicator for plotting
			ChartPlots[ToString()] = new PlotSeries("line");
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public override void AddToPlots(int currentBar)
		{
			base.AddToPlots(currentBar);

			long ticks = UtilityMethods.UnixTicks(Data.Dates[currentBar]);
			double value = Avg[currentBar];
			AddValueToPlot(ToString(), ticks, Math.Round(value, 2));
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
				Avg[currentBar] = Data.Close[currentBar];
			}
			else
			{
				double last = Avg[currentBar - 1] * Math.Min(currentBar, _period);

				if (currentBar >= _period)
				{
					Avg[currentBar] = (last + Data.Close[currentBar] - Data.Close[currentBar - _period]) / Math.Min(currentBar, _period);
				}
				else
				{
					Avg[currentBar] = (last + Data.Close[currentBar]) / (Math.Min(currentBar, _period) + 1);
				}
			}
		}
	}
}
