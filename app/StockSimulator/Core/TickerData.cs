﻿using System;
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
		public List<double> HigherTimeframeTrend { get; set; }
		public Dictionary<string, List<double>> HigherTimeframeValues { get; set; }
		public int NumBars { get; set; }
		public TickerExchangePair TickerAndExchange { get; set; }
		public double TickSize { get { return 0.01; } }

		public static readonly string[] HigherTimeframeValueStrings = 
		{
			"Sma",
			"Atr",
			"KeltnerUpper",
			"KeltnerMidline",
			"KeltnerLower",
			"DtoscSK",
			"DtoscSD",
			"Close"
		};

		// For serialization
		[JsonProperty("price")]
		public List<List<object>> PriceData { get; set; }

		[JsonProperty("volume")]
		public List<List<object>> VolumeData { get; set; }

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
			Volume = new List<long>();

			// Extras.
			Typical = new List<double>();
			Median = new List<double>();
			HigherTimeframeTrend = new List<double>();
			HigherTimeframeValues = new Dictionary<string, List<double>>();

			for (int i = 0; i < HigherTimeframeValueStrings.Length; i++)
			{
			 HigherTimeframeValues[HigherTimeframeValueStrings[i]] = new List<double>();
			}
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
				Volume.InsertRange(0, otherData.Volume.GetRange(0, copyEndIndex));

				// Extras
				Typical.InsertRange(0, otherData.Typical.GetRange(0, copyEndIndex));
				Median.InsertRange(0, otherData.Median.GetRange(0, copyEndIndex));
				HigherTimeframeTrend.InsertRange(0, otherData.HigherTimeframeTrend.GetRange(0, copyEndIndex));

				for (int i = 0; i < HigherTimeframeValueStrings.Length; i++)
				{
					string key = HigherTimeframeValueStrings[i];
					HigherTimeframeValues[key].InsertRange(0, otherData.HigherTimeframeValues[key].GetRange(0, copyEndIndex));					
				}
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
				Volume.AddRange(otherData.Volume.GetRange(copyStartIndex, otherData.Volume.Count - copyStartIndex));

				// Extras
				Typical.AddRange(otherData.Typical.GetRange(copyStartIndex, otherData.Typical.Count - copyStartIndex));
				Median.AddRange(otherData.Median.GetRange(copyStartIndex, otherData.Median.Count - copyStartIndex));
				HigherTimeframeTrend.AddRange(otherData.HigherTimeframeTrend.GetRange(copyStartIndex, otherData.HigherTimeframeTrend.Count - copyStartIndex));

				for (int i = 0; i < HigherTimeframeValueStrings.Length; i++)
				{
					string key = HigherTimeframeValueStrings[i];
					HigherTimeframeValues[key].AddRange(otherData.HigherTimeframeValues[key].GetRange(copyStartIndex, otherData.HigherTimeframeValues[key].Count - copyStartIndex));
				}
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

			if (copyStartIndex != -1)
			{
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
				copyData.Volume.AddRange(Volume.GetRange(copyStartIndex, amountToCopy));

				// Extras
				copyData.Typical.AddRange(Typical.GetRange(copyStartIndex, amountToCopy));
				copyData.Median.AddRange(Median.GetRange(copyStartIndex, amountToCopy));
				copyData.HigherTimeframeTrend.AddRange(HigherTimeframeTrend.GetRange(copyStartIndex, amountToCopy));

				for (int i = 0; i < HigherTimeframeValueStrings.Length; i++)
				{
					string key = HigherTimeframeValueStrings[i];
					copyData.HigherTimeframeValues[key].AddRange(HigherTimeframeValues[key].GetRange(copyStartIndex, amountToCopy));
				}

				copyData.Start = copyData.Dates[0];
				copyData.End = copyData.Dates[copyData.Dates.Count - 1];
				copyData.NumBars = copyData.Dates.Count;
				copyData.SaveDates();
			}

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
				Volume[i] = 0;

				// Extras
				Typical[i] = 0;
				Median[i] = 0;
				HigherTimeframeTrend[i] = Order.OrderType.Long;

				for (int j = 0; j < HigherTimeframeValueStrings.Length; j++)
				{
					string key = HigherTimeframeValueStrings[j];
					HigherTimeframeValues[key][i] = 0;
				}
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
				output += Volume[i].ToString() + ',';

				// Extras
				output += Typical[i].ToString() + ',';
				output += Median[i].ToString() + ',';
				output += HigherTimeframeTrend[i].ToString() + ',';

				for (int j = 0; j < HigherTimeframeValueStrings.Length; j++)
				{
					string key = HigherTimeframeValueStrings[j];
					output += HigherTimeframeValues[key][i].ToString() + ',';					
				}

				// End with a new line so it's easier to view raw
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
		}

		/// <summary>
		/// Releases the resources allocated when prepping for serialization.
		/// </summary>
		public void FreeResourcesAfterSerialization()
		{
			PriceData = null;
			VolumeData = null;
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
			return _dateToBar != null && _dateToBar.ContainsKey(date) ? _dateToBar[date] : -1;
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
	}
}
