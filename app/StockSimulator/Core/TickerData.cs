using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// Holds all the data for a symbol (stock, whatever)
	/// </summary>
	public class TickerData
	{
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public List<DateTime> Dates { get; set; }
		public List<double> Open { get; set; }
		public List<double> Close { get; set; }
		public List<double> High { get; set; }
		public List<double> Low { get; set; }
		public List<long> Volume { get; set; }

		// TODO: add things like the median and such and such.

		/// <summary>
		/// Creates an empty object.
		/// </summary>
		public TickerData()
		{
			Start = DateTime.Now;
			End = DateTime.Now;
		}

		/// <summary>
		/// Creates a new object to hold all the data.
		/// </summary>
		/// <param name="start">Starting date of the data</param>
		/// <param name="end">Ending date of the data</param>
		public TickerData(DateTime start, DateTime end)
		{
			Start = start;
			End = end;

			Dates = new List<DateTime>();
			Open = new List<double>();
			Close = new List<double>();
			High = new List<double>();
			Low = new List<double>();
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
				Volume.AddRange(otherData.Volume.GetRange(copyStartIndex, otherData.Volume.Count - copyStartIndex));
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
	}
}
