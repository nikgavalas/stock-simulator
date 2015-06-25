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
	/// Plots fibonacci price and time zones on the price window
	/// </summary>
	public class FibonacciZones : Indicator
	{
		public List<double> Zone38 { get; set; }
		public List<double> Zone50 { get; set; }
		public List<double> Zone62 { get; set; }

		public FibonacciZones(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			Zone38 = Enumerable.Repeat(0d, Data.NumBars).ToList();
			Zone50 = Enumerable.Repeat(0d, Data.NumBars).ToList();
			Zone62 = Enumerable.Repeat(0d, Data.NumBars).ToList();
		}

		/// <summary>
		/// This indicator is plotted on the price bars.
		/// </summary>
		[JsonProperty("plotOnPrice")]
		public override bool PlotOnPrice
		{
			get { return true; }
		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"Rsi3m3"
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
			return "FibonacciZones";
		}

		/// <summary>
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the indicator for plotting
			PlotSeries plot38 = new PlotSeries("line");
			PlotSeries plot50 = new PlotSeries("line");
			PlotSeries plot62 = new PlotSeries("line");
			ChartPlots["38% Zone"] = plot38;
			ChartPlots["50% Zone"] = plot50;
			ChartPlots["62% Zone"] = plot62;

			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long ticks = UtilityMethods.UnixTicks(Data.Dates[i]);
				plot38.PlotData.Add(new List<object>()
				{
					ticks,
					Zone38[i] > 0.0 ? (object)Math.Round(Zone38[i], 2) : null
				});

				plot50.PlotData.Add(new List<object>()
				{
					ticks,
					Zone50[i] > 0.0 ? (object)Math.Round(Zone50[i], 2) : null
				});

				plot62.PlotData.Add(new List<object>()
				{
					ticks,
					Zone62[i] > 0.0 ? (object)Math.Round(Zone62[i], 2) : null
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

			if (currentBar < 2)
			{
				return;
			}

			// We'll use this indicator to tell us when the last cycles occured.
			Rsi3m3 rsi = (Rsi3m3)Dependents[0];

			double b = 0.0;
			double a = 0.0;
			int bBar = 0;
			int aBar = 0;

			// Higher trend information determines what order we are looking for peaks
			// and valleys to calculate the retracement values.
			// The algrithm is to find the price difference between the two cycle
			// points and then calculate the ratios from that difference.
			if (Data.HigherTimeframeTrend[currentBar] == Order.OrderType.Long)
			{
				// Don't do anything unless we have the top of the cycle to work with.
				if (DataSeries.IsAbove(rsi.Value, 70, currentBar, 2) != -1 && UtilityMethods.IsPeak(rsi.Value, currentBar))
				{
					bBar = currentBar - 1;
					b = Data.High[bBar];

					// Find the last bottom cycle now for the difference.
					for (int i = currentBar - 1; i >= 2; i--)
					{
						if (DataSeries.IsBelow(rsi.Value, 30, i, 2) != -1 && UtilityMethods.IsValley(rsi.Value, i))
						{
							aBar = i - 1;
							a = Data.Low[aBar];
							break;
						}
					}

					// We have the two points needed to calculate our zones.
					if (a > 0.0 && b > a)
					{
						SetZoneLines(a, b, aBar, bBar, Order.OrderType.Long);
					}
				}
			}
			// Its always one direction or the other so no need to else if
			else
			{
				// Don't do anything unless we have the bottom of the cycle to work with.
				if (DataSeries.IsBelow(rsi.Value, 30, currentBar, 2) != -1 && UtilityMethods.IsValley(rsi.Value, currentBar))
				{
					bBar = currentBar - 1;
					b = Data.Low[bBar];

					// Find the last top cycle now for the difference.
					for (int i = currentBar - 1; i >= 2; i--)
					{
						if (DataSeries.IsAbove(rsi.Value, 70, i, 2) != -1 && UtilityMethods.IsPeak(rsi.Value, i))
						{
							aBar = i - 1;
							a = Data.High[aBar];
							break;
						}
					}

					// We have the two points needed to calculate our zones.
					if (a > 0.0 && b < a)
					{
						SetZoneLines(a, b, aBar, bBar, Order.OrderType.Short);
					}
				}
			}
		}

		/// <summary>
		/// Sets the values for the buy zone which is a start and end bar, and the fibonacci zones
		/// </summary>
		/// <param name="a">First reversal price</param>
		/// <param name="b">Second reversal price</param>
		/// <param name="aBar">Bar the first reversal was found</param>
		/// <param name="bBar">Bar the second reversal was found</param>
		/// <param name="orderType">Type of order we're looking for</param>
		private void SetZoneLines(double a, double b, int aBar, int bBar, double orderType)
		{
			// Get the price and timing zones. Timing zones are always forward in time.
			double[] priceZones = GetPriceZones(a, b, orderType);
			int[] timingZones = GetTimingZones(aBar, bBar);

			// Only care about the first and last ratio for the timing.
			int startBar = timingZones[0];
			int endBar = timingZones[1];

			if (startBar < Data.NumBars)
			{
				int lastBar = endBar < Data.NumBars ? endBar : Data.NumBars - 1;
				for (int i = startBar; i < lastBar; i++)
				{
					Zone38[i] = priceZones[0];
					Zone50[i] = priceZones[1];
					Zone62[i] = priceZones[2];
				}
			}
		}

		/// <summary>
		/// Returns the major fibonacci zone values.
		/// </summary>
		/// <param name="a">First reversal price</param>
		/// <param name="b">Second reversal price</param>
		/// <param name="orderType">What market direction to calculate for</param>
		/// <returns>Computed zone values</returns>
		private double[] GetPriceZones(double a, double b, double orderType)
		{
			double[] zones = new double[3];
			zones[0] = GetZone(a, b, 0.382, orderType);
			zones[1] = GetZone(a, b, 0.500, orderType);
			zones[2] = GetZone(a, b, 0.618, orderType);
			return zones;
		}

		/// <summary>
		/// Returns the major fibonacci zone values.
		/// </summary>
		/// <param name="a">First reversal price</param>
		/// <param name="b">Second reversal price</param>
		/// <returns>Computed zone values</returns>
		private int[] GetTimingZones(int a, int b)
		{
			int[] zones = new int[2];
			zones[0] = Convert.ToInt32(b + (double)(b - a) * 0.382);
			zones[1] = Convert.ToInt32(b + (double)(b - a) * 0.618);
			return zones;
		}

		/// <summary>
		/// Returns the fibonacci zones based on the input numbers.
		/// http://www.forexfibonacci.com/calculate_fibonacci_levels/04/
		/// </summary>
		/// <param name="a">First reversal price</param>
		/// <param name="b">Second reversal price</param>
		/// <param name="percent">Which percent to calculate for</param>
		/// <param name="orderType">What market direction to calculate for</param>
		/// <returns>Computed zone value</returns>
		private double GetZone(double a, double b, double percent, double orderType)
		{
			if (orderType == Order.OrderType.Long)
			{
				return b - (b - a) * percent;
			}
			else
			{
				return b + (a - b) * percent;
			}
		}
	}
}
