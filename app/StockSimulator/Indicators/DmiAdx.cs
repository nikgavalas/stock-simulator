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
	/// DmiAdx
	/// </summary>
	class DmiAdx : Indicator
	{
		public List<double> Value { get; set; }
		public List<double> DmiPlus { get; set; }
		public List<double> DmiMinus { get; set; }
		public List<double> Adx { get; set; }

		private List<double> dmMinus;
		private List<double> dmPlus;
		private List<double> tr;

		#region Configurables
		public int Period
		{
			get { return _Period; }
			set { _Period = value; }
		}

		private int _Period = 14;
		#endregion
	
		
		public DmiAdx(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				new Adx(tickerData) { Period = Period }
			};

			Value = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			DmiPlus = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			DmiMinus = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			Adx = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			dmMinus = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			dmPlus = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
			tr = UtilityMethods.CreateList<double>(Data.NumBars, 0d);
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "DmiAdx" + Period.ToString();
		}

		/// <summary>
		/// Creates the plots for the data to be added to.
		/// </summary>
		public override void CreatePlots()
		{
			base.CreatePlots();

			// Add the indicator for plotting
			ChartPlots["Dmi+"] = new PlotSeries("line");
			ChartPlots["Dmi-"] = new PlotSeries("line");
			ChartPlots["Adx"] = new PlotSeries("line");
		}

		/// <summary>
		/// Adds data to the created plots for the indicator at the current bar.
		/// </summary>
		/// <param name="currentBar"></param>
		public override void AddToPlots(int currentBar)
		{
			base.AddToPlots(currentBar);

			long ticks = UtilityMethods.UnixTicks(Data.Dates[currentBar]);

			PlotSeries line = (PlotSeries)ChartPlots["Dmi+"];
			line.PlotData.Add(new List<object>()
			{
				ticks,
				Math.Round(DmiPlus[currentBar], 2)
			});

			line = (PlotSeries)ChartPlots["Dmi-"];
			line.PlotData.Add(new List<object>()
			{
				ticks,
				Math.Round(DmiMinus[currentBar], 2)
			});

			line = (PlotSeries)ChartPlots["Adx"];
			line.PlotData.Add(new List<object>()
			{
				ticks,
				Math.Round(Adx[currentBar], 2)
			});
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			Adx[currentBar] = ((Adx)_dependents[0]).Value[currentBar];

			if (currentBar == 0)
			{
				dmMinus[currentBar] = 0;
				dmPlus[currentBar] = 0;
				tr[currentBar] = Data.High[currentBar] - Data.Low[currentBar];
				Value[currentBar] = 0;
				DmiPlus[currentBar] = 0;
				DmiMinus[currentBar] = 0;
			}
			else
			{
				dmMinus[currentBar] = Data.Low[currentBar - 1] - Data.Low[currentBar] > Data.High[currentBar] - Data.High[currentBar - 1] ? Math.Max(Data.Low[currentBar - 1] - Data.Low[currentBar], 0) : 0;
				dmPlus[currentBar] = Data.High[currentBar] - Data.High[currentBar - 1] > Data.Low[currentBar - 1] - Data.Low[currentBar] ? Math.Max(Data.High[currentBar] - Data.High[currentBar - 1], 0) : 0;
				tr[currentBar] = Math.Max(Data.High[currentBar] - Data.Low[currentBar], Math.Max(Math.Abs(Data.High[currentBar] - Data.Close[currentBar - 1]), Math.Abs(Data.Low[currentBar] - Data.Close[currentBar - 1])));

				double diPlus = UtilityMethods.Sma(tr, currentBar, Period) == 0 ? 0 : UtilityMethods.Sma(dmPlus, currentBar, Period) / UtilityMethods.Sma(tr, currentBar, Period);
				double diMinus = UtilityMethods.Sma(tr, currentBar, Period) == 0 ? 0 : UtilityMethods.Sma(dmMinus, currentBar, Period) / UtilityMethods.Sma(tr, currentBar, Period);

				DmiPlus[currentBar] = diPlus * 100.0;
				DmiMinus[currentBar] = diMinus * 100.0;
				Value[currentBar] = (diPlus + diMinus == 0) ? 0 : (diPlus - diMinus) / (diPlus + diMinus);
			}
		}
	}
}
