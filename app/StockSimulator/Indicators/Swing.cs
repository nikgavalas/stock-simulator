using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core;
using System.Collections;
using Newtonsoft.Json;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// CCI indicator
	/// </summary>
	class Swing : Indicator
	{
		public List<double> SwingLowPlot { get; set; }
		public List<double> SwingHighPlot { get; set; }

		private int _strength = 1;
		private double currentSwingHigh = 0;
		private double currentSwingLow = 0;
		private ArrayList lastHighCache;
		private double lastSwingHighValue = 0;
		private ArrayList lastLowCache;
		private double lastSwingLowValue = 0;
		private int saveCurrentBar = -1;
		private int strength = 5;
		private List<double> swingHighSeries;
		private List<double> swingHighSwings;
		private List<double> swingLowSeries;
		private List<double> swingLowSwings;

		public Swing(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
			SwingLowPlot = Enumerable.Repeat(0d, Data.NumBars).ToList();
			SwingHighPlot = Enumerable.Repeat(0d, Data.NumBars).ToList();

			swingHighSeries = Enumerable.Repeat(0d, Data.NumBars).ToList();
			swingHighSwings = Enumerable.Repeat(0d, Data.NumBars).ToList();
			swingLowSeries = Enumerable.Repeat(0d, Data.NumBars).ToList();
			swingLowSwings = Enumerable.Repeat(0d, Data.NumBars).ToList();

			lastHighCache = new ArrayList();
			lastLowCache = new ArrayList();

			_strength = Simulator.Config.TrendStrength;
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "Swing" + _strength.ToString();
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
		/// Save the indicator data in a serialization friendly way.
		/// </summary>
		public override void PrepareForSerialization()
		{
			base.PrepareForSerialization();

			// Add the rsi for plotting
			PlotSeries plotHigh = new PlotSeries("line");
			PlotSeries plotLow = new PlotSeries("line");
			ChartPlots[ToString() + "High"] = plotHigh;
			ChartPlots[ToString() + "Low"] = plotLow;

			for (int i = 0; i < Data.Dates.Count; i++)
			{
				long dateTicks = ExtensionMethods.UnixTicks(Data.Dates[i]);
				if (SwingHighPlot[i] > 0)
				{
					plotHigh.PlotData.Add(new List<object>()
					{
						dateTicks,
						Math.Round(SwingHighPlot[i], 2)
					});
				}

				if (SwingLowPlot[i] > 0)
				{
					plotLow.PlotData.Add(new List<object>()
					{
						dateTicks,
						Math.Round(SwingLowPlot[i], 2)
					});
				}
			}
		}

		/// <summary>
		/// Returns the number of bars ago a swing low occurred. Returns a value of -1 if a swing low is not found within the look back period.
		/// </summary>
		/// <param name="currentBar"></param>
		/// <param name="barsAgo"></param>
		/// <param name="instance"></param>
		/// <param name="lookBackPeriod"></param>
		/// <returns></returns>
		public int SwingLowBar(int currentBar, int barsAgo, int instance, int lookBackPeriod)
		{
			if (instance < 1)
			{
				throw new Exception(GetType().Name + ".SwingLowBar: instance must be greater/equal 1 but was " + instance);
			}
			else if (barsAgo < 0)
			{
				throw new Exception(GetType().Name + ".SwingLowBar: barsAgo must be greater/equal 0 but was " + barsAgo);
			}
			else if (barsAgo >= Simulator.NumberOfBars)
			{
				throw new Exception(GetType().Name + ".SwingLowBar: barsAgo out of valid range 0 through " + (Simulator.NumberOfBars - 1) + ", was " + barsAgo + ".");
			}

			for (int idx = currentBar - barsAgo - strength; idx >= currentBar - barsAgo - strength - lookBackPeriod; idx--)
			{
				if (idx < 0)
				{
					return -1;
				}

				if (idx >= swingLowSwings.Count)
				{
					continue;
				}

				if (swingLowSwings[idx].Equals(0.0))
				{
					continue;
				}

				if (instance == 1) // 1-based, < to be save
				{
					return currentBar - idx;
				}

				instance--;
			}

			return -1;
		}

		/// <summary>
		/// Returns the number of bars ago a swing high occurred. Returns a value of -1 if a swing high is not found within the look back period.
		/// </summary>
		/// <param name="currentBar"></param>
		/// <param name="barsAgo"></param>
		/// <param name="instance"></param>
		/// <param name="lookBackPeriod"></param>
		/// <returns></returns>
		public int SwingHighBar(int currentBar, int barsAgo, int instance, int lookBackPeriod)
		{
			if (instance < 1)
				throw new Exception(GetType().Name + ".SwingHighBar: instance must be greater/equal 1 but was " + instance);
			else if (barsAgo < 0)
				throw new Exception(GetType().Name + ".SwingHighBar: barsAgo must be greater/equal 0 but was " + barsAgo);
			else if (barsAgo >= Simulator.NumberOfBars)
				throw new Exception(GetType().Name + ".SwingHighBar: barsAgo out of valid range 0 through " + (Simulator.NumberOfBars - 1) + ", was " + barsAgo + ".");

			for (int idx = currentBar - barsAgo - strength; idx >= currentBar - barsAgo - strength - lookBackPeriod; idx--)
			{
				if (idx < 0)
				{
					return -1;
				}

				if (idx >= swingHighSwings.Count)
				{
					continue;
				}

				if (swingHighSwings[idx].Equals(0.0))
				{
					continue;
				}

				if (instance <= 1) // 1-based, < to be save
				{
					return currentBar - idx;
				}

				instance--;
			}

			return -1;
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (saveCurrentBar != currentBar)
			{
				swingHighSwings[currentBar] = 0;
				swingLowSwings[currentBar] = 0;

				swingHighSeries[currentBar] = 0;
				swingLowSeries[currentBar] = 0;

				lastHighCache.Add(Data.High[currentBar]);
				if (lastHighCache.Count > (2 * strength) + 1)
				{
					lastHighCache.RemoveAt(0);
				}

				lastLowCache.Add(Data.Low[currentBar]);
				if (lastLowCache.Count > (2 * strength) + 1)
				{
					lastLowCache.RemoveAt(0);
				}

				if (lastHighCache.Count == (2 * strength) + 1)
				{
					bool isSwingHigh = true;
					double swingHighCandidateValue = (double)lastHighCache[strength];
					for (int i = 0; i < strength; i++)
					{
						if ((double)lastHighCache[i] >= swingHighCandidateValue - double.Epsilon)
						{
							isSwingHigh = false;
						}
					}

					for (int i = strength + 1; i < lastHighCache.Count; i++)
					{
						if ((double)lastHighCache[i] > swingHighCandidateValue - double.Epsilon)
						{
							isSwingHigh = false;
						}
					}

					swingHighSwings[currentBar - strength] = isSwingHigh ? swingHighCandidateValue : 0.0;
					if (isSwingHigh)
					{
						lastSwingHighValue = swingHighCandidateValue;
					}

					if (isSwingHigh)
					{
						currentSwingHigh = swingHighCandidateValue;
						for (int i = 0; i <= strength; i++)
						{
							SwingHighPlot[currentBar - i] = currentSwingHigh;
						}
					}
					else if (Data.High[currentBar] > currentSwingHigh)
					{
						currentSwingHigh = 0.0;
						// ?
						//SwingHighPlot.Reset();
					}
					else
					{
						SwingHighPlot[currentBar] = currentSwingHigh;
					}

					if (isSwingHigh)
					{
						for (int i = 0; i <= strength; i++)
						{
							swingHighSeries[currentBar - i] = lastSwingHighValue;
						}
					}
					else
					{
						swingHighSeries[currentBar] = lastSwingHighValue;
					}
				}

				if (lastLowCache.Count == (2 * strength) + 1)
				{
					bool isSwingLow = true;
					double swingLowCandidateValue = (double)lastLowCache[strength];
					for (int i = 0; i < strength; i++)
					{
						if ((double)lastLowCache[i] <= swingLowCandidateValue + double.Epsilon)
						{
							isSwingLow = false;
						}
					}

					for (int i = strength + 1; i < lastLowCache.Count; i++)
					{
						if ((double)lastLowCache[i] < swingLowCandidateValue + double.Epsilon)
						{
							isSwingLow = false;
						}
					}

					swingLowSwings[currentBar - strength] = isSwingLow ? swingLowCandidateValue : 0.0;
					if (isSwingLow)
					{
						lastSwingLowValue = swingLowCandidateValue;
					}

					if (isSwingLow)
					{
						currentSwingLow = swingLowCandidateValue;
						for (int i = 0; i <= strength; i++)
						{
							SwingLowPlot[currentBar - i] = currentSwingLow;
						}
					}
					else if (Data.Low[currentBar] < currentSwingLow)
					{
						currentSwingLow = double.MaxValue;
						// ?
						//SwingLowPlot.Reset();
					}
					else
					{
						SwingLowPlot[currentBar] = currentSwingLow;
					}

					if (isSwingLow)
					{
						for (int i = 0; i <= strength; i++)
						{
							swingLowSeries[currentBar - i] = lastSwingLowValue;
						}
					}
					else
					{
						swingLowSeries[currentBar] = lastSwingLowValue;
					}
				}

				saveCurrentBar = currentBar;
			}
			else
			{
				if (Data.High[currentBar] > Data.High[currentBar - strength] && swingHighSwings[currentBar - strength] > 0.0)
				{
					swingHighSwings[currentBar - strength] = 0.0;
					for (int i = 0; i <= strength; i++)
					{
						// ?
						//SwingHighPlot.Reset(i);
					}
					currentSwingHigh = 0.0;
				}
				else if (Data.High[currentBar] > Data.High[currentBar - strength] && currentSwingHigh != 0.0)
				{
					// ?
					//SwingHighPlot.Reset();
					currentSwingHigh = 0.0;
				}
				else if (Data.High[currentBar] <= currentSwingHigh)
				{
					SwingHighPlot[currentBar] = currentSwingHigh;
				}

				if (Data.Low[currentBar] < Data.Low[currentBar - strength] && swingLowSwings[currentBar - strength] > 0.0)
				{
					swingLowSwings[currentBar - strength] = 0.0;
					for (int i = 0; i <= strength; i++)
					{
						// ?
						//SwingLowPlot.Reset(i);
					}
					currentSwingLow = double.MaxValue;
				}
				else if (Data.Low[currentBar] < Data.Low[currentBar - strength] && currentSwingLow != double.MaxValue)
				{
					// ?
					//SwingLowPlot.Reset();
					currentSwingLow = double.MaxValue;
				}
				else if (Data.Low[currentBar] >= currentSwingLow)
				{
					SwingLowPlot[currentBar] = currentSwingLow;
				}
			}

		}
	}

}
