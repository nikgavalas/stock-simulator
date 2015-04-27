using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StockSimulator.Indicators;

namespace StockSimulator.Core
{
	/// <summary>
	/// Holds all the data for a symbol (stock, whatever)
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class TickerData
	{
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public List<DateTime> Dates { get; set; }
		public List<double> Open { get; set; }
		public List<double> Close { get; set; }
		public List<double> High { get; set; }
		public List<double> Low { get; set; }
		public List<double> Typical { get; set; }
		public List<double> Median { get; set; }
		public List<long> Volume { get; set; }
		public List<Order.OrderType> HigherTimeframeMomentum { get; set; }
		public int NumBars { get; set; }
		public TickerExchangePair TickerAndExchange { get; set; }
		public double TickSize { get { return 0.01; } }

		// For serialization
		[JsonProperty("price")]
		public List<List<object>> PriceData { get; set; }

		[JsonProperty("volume")]
		public List<List<object>> VolumeData { get; set; }

		[JsonProperty("higherTimeframe")]
		public TickerData HigherTimeframe { get; set; }

		[JsonProperty("higherTimeframeIndicator")]
		public Stochastics HigherTimeframeIndicator { get; set; }

		/// <summary>
		/// A map of a date to a current bar. We need this because there seems to be extra 
		/// trading dates on NYSE that don't exist on NASDAQ.
		/// For example, NYSE has 4/1/2010 while NASDAQ's first date in April is 4/5/2010.
		/// So we use this to get the current bar from a date.
		/// </summary>
		private Dictionary<DateTime, int> _dateToBar;
		
		/// <summary>
		/// Creates a new object to hold all the data.
		/// </summary>
		/// <param name="start">Starting date of the data</param>
		/// <param name="end">Ending date of the data</param>
		public TickerData(TickerExchangePair tickerAndExchange)
		{
			TickerAndExchange = tickerAndExchange;
			Start = DateTime.Now;
			End = DateTime.Now;

			Dates = new List<DateTime>();
			Open = new List<double>();
			Close = new List<double>();
			High = new List<double>();
			Low = new List<double>();
			Typical = new List<double>();
			Median = new List<double>();
			Volume = new List<long>();

			HigherTimeframeMomentum = null;
			HigherTimeframe = null;
			HigherTimeframeIndicator = null;
		}

		/// <summary>
		/// Appends the other data to the data already in this class. It doesn't overwrite data
		/// for existing dates, so it can only prepend data to the start or append to the end.
		/// </summary>
		/// <param name="otherData">Data to append</param>
		public void AppendData(TickerData otherData)
		{
			// Prepend
			if (otherData.Start < Start)
			{
				// Find the index in the other data where this data starts.
				int copyEndIndex;
				for (copyEndIndex = 0; copyEndIndex < otherData.Dates.Count; copyEndIndex++)
				{
					if (otherData.Dates[copyEndIndex] >= Start)
					{
						break;
					}
				}

				// Insert all the new data at the front of the existing data.
				Dates.InsertRange(0, otherData.Dates.GetRange(0, copyEndIndex));
				Open.InsertRange(0, otherData.Open.GetRange(0, copyEndIndex));
				Close.InsertRange(0, otherData.Close.GetRange(0, copyEndIndex));
				High.InsertRange(0, otherData.High.GetRange(0, copyEndIndex));
				Low.InsertRange(0, otherData.Low.GetRange(0, copyEndIndex));
				Typical.InsertRange(0, otherData.Typical.GetRange(0, copyEndIndex));
				Median.InsertRange(0, otherData.Median.GetRange(0, copyEndIndex));
				Volume.InsertRange(0, otherData.Volume.GetRange(0, copyEndIndex));
			}

			// Append
			if (otherData.End > End)
			{
				// Find the index where the other data passes the end of the existing data.
				int copyStartIndex;
				for (copyStartIndex = 0; copyStartIndex < otherData.Dates.Count; copyStartIndex++)
				{
					if (otherData.Dates[copyStartIndex] > End)
					{
						break;
					}
				}

				// Append the new data to the end of the existing data.
				Dates.AddRange(otherData.Dates.GetRange(copyStartIndex, otherData.Dates.Count - copyStartIndex));
				Open.AddRange(otherData.Open.GetRange(copyStartIndex, otherData.Open.Count - copyStartIndex));
				Close.AddRange(otherData.Close.GetRange(copyStartIndex, otherData.Close.Count - copyStartIndex));
				High.AddRange(otherData.High.GetRange(copyStartIndex, otherData.High.Count - copyStartIndex));
				Low.AddRange(otherData.Low.GetRange(copyStartIndex, otherData.Low.Count - copyStartIndex));
				Typical.AddRange(otherData.Typical.GetRange(copyStartIndex, otherData.Typical.Count - copyStartIndex));
				Median.AddRange(otherData.Median.GetRange(copyStartIndex, otherData.Median.Count - copyStartIndex));
				Volume.AddRange(otherData.Volume.GetRange(copyStartIndex, otherData.Volume.Count - copyStartIndex));
			}

			Start = Dates[0];
			End = Dates[Dates.Count - 1];
			NumBars = Dates.Count;
			SaveDates();
		}

		/// <summary>
		/// Gets a copy of this data from a date, to a date.
		/// </summary>
		/// <param name="start">Date to start from</param>
		/// <param name="end">Date to end at</param>
		/// <returns>New object with the data from the requested dates</returns>
		public TickerData SubSet(DateTime start, DateTime end)
		{
			TickerData copyData = new TickerData(TickerAndExchange);

			int copyStartIndex = -1;
			int copyEndIndex = -1;
			for (int i = 0; i < Dates.Count; i++)
			{
				if (copyStartIndex == -1 && Dates[i] >= start)
				{
					copyStartIndex = i;
				}
				if (copyEndIndex == -1 && Dates[i] >= end)
				{
					copyEndIndex = i;
				}

				if (copyStartIndex > -1 && copyEndIndex > -1)
				{
					break;
				}
			}

			if (copyEndIndex == -1 && end > Dates.Last())
			{
				copyEndIndex = Dates.Count - 1;
			}

			int amountToCopy = (copyEndIndex - copyStartIndex) + 1;
			copyData.Dates.AddRange(Dates.GetRange(copyStartIndex, amountToCopy));
			copyData.Open.AddRange(Open.GetRange(copyStartIndex, amountToCopy));
			copyData.Close.AddRange(Close.GetRange(copyStartIndex, amountToCopy));
			copyData.High.AddRange(High.GetRange(copyStartIndex, amountToCopy));
			copyData.Low.AddRange(Low.GetRange(copyStartIndex, amountToCopy));
			copyData.Typical.AddRange(Typical.GetRange(copyStartIndex, amountToCopy));
			copyData.Median.AddRange(Median.GetRange(copyStartIndex, amountToCopy));
			copyData.Volume.AddRange(Volume.GetRange(copyStartIndex, amountToCopy));

			copyData.Start = copyData.Dates[0];
			copyData.End = copyData.Dates[copyData.Dates.Count - 1];
			copyData.NumBars = copyData.Dates.Count;
			copyData.SaveDates();
			return copyData;
		}

		/// <summary>
		/// Sets all the prices to 0 leaving the dates intact.
		/// </summary>
		public void ZeroPrices()
		{
			for (int i = 0; i < Dates.Count; i++)
			{
				Open[i] = 0;
				Close[i] = 0;
				High[i] = 0;
				Low[i] = 0;
				Typical[i] = 0;
				Median[i] = 0;
				Volume[i] = 0;
			}
		}

		// Commented out because it really slows down debugging. But there may be a time 
		// when this is useful to have.
		/// <summary>
		/// Returns a string with one data set per line.
		/// </summary>
		/// <returns>String of data</returns>
		public string WriteToString()
		{
			string output = "";
			for (int i = 0; i < Dates.Count; i++)
			{
				output += UtilityMethods.UnixTicks(Dates[i]).ToString() + ',';
				output += Open[i].ToString() + ',';
				output += High[i].ToString() + ',';
				output += Low[i].ToString() + ',';
				output += Close[i].ToString() + ',';
				output += Volume[i].ToString();
				output += Environment.NewLine;
			}

			return output;
		}

		/// <summary>
		/// Inits the list so they can be outputted in a json/highcharts friendly way.
		/// </summary>
		public void PrepareForSerialization()
		{
			PriceData = new List<List<object>>();
			VolumeData = new List<List<object>>();

			for (int i = 0; i < Dates.Count; i++)
			{
				PriceData.Add(new List<object>()
				{
					UtilityMethods.UnixTicks(Dates[i]),
					Math.Round(Open[i], 2),
					Math.Round(High[i], 2),
					Math.Round(Low[i], 2),
					Math.Round(Close[i], 2),
				});

				VolumeData.Add(new List<object>()
				{
					UtilityMethods.UnixTicks(Dates[i]),
					Volume[i]
				});
			}

			if (HigherTimeframe != null)
			{
				HigherTimeframe.PrepareForSerialization();
				HigherTimeframeIndicator.PrepareForSerialization();
			}
		}

		/// <summary>
		/// Releases the resources allocated when prepping for serialization.
		/// </summary>
		public void FreeResourcesAfterSerialization()
		{
			PriceData = null;
			VolumeData = null;

			if (HigherTimeframe != null)
			{
				HigherTimeframe.FreeResourcesAfterSerialization();
				HigherTimeframeIndicator.FreeResourcesAfterSerialization();
			}
		}

		/// <summary>
		/// Returns if this is a valid bar of data.
		/// Some bars just have zeros which is what we want for the simulation
		/// but not what we want for calculating indicators and strategies.
		/// </summary>
		/// <param name="bar">Bar to check</param>
		/// <returns>True if the open and close price are more than zero</returns>
		public bool IsValidBar(int bar)
		{
			return Close[bar] > 0 && Open[bar] > 0;
		}

		/// <summary>
		/// Used to get the bar for the simulation from a date.
		/// </summary>
		/// <param name="date">Date of the bar requested</param>
		/// <returns>The bar if the date exists, otherwise -1</returns>
		public int GetBar(DateTime date)
		{
			return _dateToBar.ContainsKey(date) ? _dateToBar[date] : -1;
		}

		/// <summary>
		/// Saves all the dates to bar mappings.
		/// </summary>
		public void SaveDates()
		{
			// Save all the dates as keys so we can find out what bar they match to.
			_dateToBar = new Dictionary<DateTime, int>();
			for (int i = 0; i < Dates.Count; i++)
			{
				_dateToBar[Dates[i]] = i;
			}
		}

		/// <summary>
		/// Uses the current ticker data and aggregates it for a higher time frame set of data.
		/// Then will run a momentum indicator on that data and compute the 4 different types
		/// of momentum states we can be in.
		/// Bull, not OB = Long orders
		/// Bull, OB = Short orders
		/// Bear, not OS = Short orders
		/// Bear, OS = Long orders
		/// </summary>
		public void CalcHigherTimeframe()
		{
			double open = 0;
			double high = 0;
			double low = 0;
			double close = 0;
			long volume = 0;
			int barCount = 0;

			// Reset the states since we'll calculate them again.
			HigherTimeframe = new TickerData(TickerAndExchange);
			HigherTimeframeMomentum = new List<Order.OrderType>();

			// Aggregate all the data into the higher timeframe.
			for (int i = 0; i < Dates.Count; i++)
			{
				// The first bar open we'll treat as the open price and set the high and low.
				// Volume gets reset as it's cumulative through all the bars.
				if (barCount == 0)
				{
					open = Open[i];
					low = Low[i];
					high = High[i];
					volume = 0;
				}

				// If this low is lower than the saved low, we have a new low. 
				// Same for high but opposite of course.
				if (Low[i] < low)
				{
					low = Low[i];
				}
				if (High[i] > high)
				{
					high = High[i];
				}

				// Move to the next bar to aggregate from.
				++barCount;
				volume += Volume[i];

				// The last bar close is treated as the close. Now it's time to save all
				// the aggregated data as one bar for the higher timeframe.
				// We also want to do this if the for loop is just about to exit. We may not
				// have the number of bars we wanted for the aggregate, but we want to at least have
				// something for the last bar. Ex. We have 5 bars set for the higher timeframe length,
				// but we've only got 3 bars of data and the for loop will end on the next iteration.
				// In that case we want to use the 3 bars we have for the data.
				if (barCount == Simulator.Config.NumBarsHigherTimeframe || (i + 1) == Dates.Count)
				{
					close = Close[i];

					HigherTimeframe.Dates.Add(Dates[i]); // Use the ending aggregated date as the date for the higher timeframe.
					HigherTimeframe.Open.Add(open);
					HigherTimeframe.High.Add(high);
					HigherTimeframe.Low.Add(low);
					HigherTimeframe.Close.Add(close);
					HigherTimeframe.Volume.Add(volume);
					HigherTimeframe.NumBars = HigherTimeframe.Dates.Count;

					// Start aggregating a new set.
					barCount = 0;
				}
			}

			// Run the indicator and save it.
			HigherTimeframeIndicator = new Stochastics(HigherTimeframe, new RunnableFactory(HigherTimeframe));
			HigherTimeframeIndicator.Run();

			// Run through all the dates and see what state we're in.
			int lowerTimeframeStartBar = 0;
			for (int i = 0; i < HigherTimeframe.Dates.Count; i++)
			{
				Order.OrderType momentumState = GetHigherTimeframeMomentumState(HigherTimeframeIndicator, i);

				// Assign the lower timeframe the state of the higher timeframe at this time.
				int lowerTimeframeEndBar = _dateToBar[HigherTimeframe.Dates[i]];
				for (int j = lowerTimeframeStartBar; j < lowerTimeframeEndBar; j++)
				{
					HigherTimeframeMomentum.Add(momentumState);
				}

				lowerTimeframeStartBar = lowerTimeframeEndBar;

				// Unsure if this is right, but it makes the bar counts add up at least.
				if (i + 1 == HigherTimeframe.Dates.Count)
				{
					HigherTimeframeMomentum.Add(momentumState);
				}
			}
		}

		/// <summary>
		/// Returns the state of the higher momentum bar. Any momentum indicator can be used here.
		/// </summary>
		/// <param name="indicator">Momentum indicator to use</param>
		/// <param name="curBar">Current bar in the momentum simulation</param>
		/// <returns>The state of the higher momentum indicator</returns>
		private Order.OrderType GetHigherTimeframeMomentumState(Stochastics indicator, int curBar)
		{
			if (DataSeries.IsAbove(indicator.K, indicator.D, curBar, 0) != -1 || indicator.K[curBar] == indicator.D[curBar])
			{
				return Order.OrderType.Long;
			}
			else if (DataSeries.IsBelow(indicator.K, indicator.D, curBar, 0) != -1)
			{
				return Order.OrderType.Short;
			}

			throw new Exception("Unknown higher momentum state");
		}
	}
}
