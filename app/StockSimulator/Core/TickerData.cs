using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
		public int NumBars { get; set; }
		public TickerExchangePair TickerAndExchange { get; set; }

		// For serialization
		[JsonProperty("price")]
		public List<List<object>> PriceData { get; set; }

		[JsonProperty("volume")]
		public List<List<object>> VolumeData { get; set; }

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

		/// <summary>
		/// Returns a string with one data set per line.
		/// </summary>
		/// <returns>String of data</returns>
		public override string ToString()
		{
			string output = "";
			for (int i = 0; i < Dates.Count; i++)
			{
				output += Dates[i].ToShortDateString() + ',';
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
					ExtensionMethods.UnixTicks(Dates[i]),
					Math.Round(Open[i], 2),
					Math.Round(High[i], 2),
					Math.Round(Low[i], 2),
					Math.Round(Close[i], 2),
				});

				VolumeData.Add(new List<object>()
				{
					ExtensionMethods.UnixTicks(Dates[i]),
					Volume[i]
				});
			}
		}
	}
}
