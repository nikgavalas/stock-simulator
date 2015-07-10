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
	/// Adx
	/// </summary>
	class Adx : Indicator
	{
		public List<double> Value { get; set; }

		private List<double> dmMinus;
		private List<double> dmPlus;
		private List<double> tr;
		private List<double> sumDmMinus;
		private List<double> sumDmPlus;
		private List<double> sumTr;

		#region Configurables
		public int Period
		{
			get { return _period; }
			set { _period = value; }
		}

		private int _period = 8;
		#endregion


		public Adx(TickerData tickerData)
			: base(tickerData)
		{
			Value = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			dmMinus = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			dmPlus = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			tr = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			sumDmMinus = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			sumDmPlus = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			sumTr = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Adx" + Period.ToString();
		}

		/// <summary>
		/// Don't plot on the charts since this is used in the DmiAdx indicator
		/// </summary>
		public override bool HasPlot
		{
			get { return false; }
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			double trueRange = Data.High[currentBar] - Data.Low[currentBar];
			if (currentBar == 0)
			{
				tr[currentBar] = trueRange;
				dmPlus[currentBar] = 0;
				dmMinus[currentBar] = 0;
				sumTr[currentBar] = tr[currentBar];
				sumDmPlus[currentBar] = dmPlus[currentBar];
				sumDmMinus[currentBar] = dmMinus[currentBar];
				Value[currentBar] = 50;
			}
			else
			{
				tr[currentBar] = Math.Max(Math.Abs(Data.Low[currentBar] - Data.Close[currentBar - 1]), Math.Max(trueRange, Math.Abs(Data.High[currentBar] - Data.Close[currentBar - 1])));
				dmPlus[currentBar] = Data.High[currentBar] - Data.High[currentBar - 1] > Data.Low[currentBar - 1] - Data.Low[currentBar] ? Math.Max(Data.High[currentBar] - Data.High[currentBar - 1], 0) : 0;
				dmMinus[currentBar] = Data.Low[currentBar - 1] - Data.Low[currentBar] > Data.High[currentBar] - Data.High[currentBar - 1] ? Math.Max(Data.Low[currentBar - 1] - Data.Low[currentBar], 0) : 0;

				if (currentBar < Period)
				{
					sumTr[currentBar] = sumTr[currentBar - 1] + tr[currentBar];
					sumDmPlus[currentBar] = sumDmPlus[currentBar - 1] + dmPlus[currentBar];
					sumDmMinus[currentBar] = sumDmMinus[currentBar - 1] + dmMinus[currentBar];
				}
				else
				{
					sumTr[currentBar] = sumTr[currentBar - 1] - sumTr[currentBar - 1] / Period + tr[currentBar];
					sumDmPlus[currentBar] = sumDmPlus[currentBar - 1] - sumDmPlus[currentBar - 1] / Period + dmPlus[currentBar];
					sumDmMinus[currentBar] = sumDmMinus[currentBar - 1] - sumDmMinus[currentBar - 1] / Period + dmMinus[currentBar];
				}

				double diPlus = 100 * (sumTr[currentBar] == 0 ? 0 : sumDmPlus[currentBar] / sumTr[currentBar]);
				double diMinus = 100 * (sumTr[currentBar] == 0 ? 0 : sumDmMinus[currentBar] / sumTr[currentBar]);
				double diff = Math.Abs(diPlus - diMinus);
				double sum = diPlus + diMinus;

				Value[currentBar] = sum == 0 ? 50 : ((Period - 1) * Value[currentBar - 1] + 100 * diff / sum) / Period;
			}
		}
	}
}
