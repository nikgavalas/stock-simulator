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
	/// Calculates the average volume over a period
	/// </summary>
	class AverageVolume : Indicator
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
		/// Creates the indicator.
		/// Add any dependents here.
		/// </summary>
		/// <param name="tickerData">Price data</param>
		public AverageVolume(TickerData tickerData)
			: base(tickerData)
		{
			Avg = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
		}

		/// <summary>
		/// Don't plot on the charts
		/// </summary>
		public override bool HasPlot
		{
			get { return false; }
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "AverageVolume";
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
				Avg[currentBar] = Data.Volume[currentBar];
			}
			else
			{
				double last = Avg[currentBar - 1] * Math.Min(currentBar, _period);

				if (currentBar >= _period)
				{
					Avg[currentBar] = (last + Data.Volume[currentBar] - Data.Volume[currentBar - _period]) / Math.Min(currentBar, _period);
				}
				else
				{
					Avg[currentBar] = (last + Data.Volume[currentBar]) / (Math.Min(currentBar, _period) + 1);
				}
			}
		}

	}
}
