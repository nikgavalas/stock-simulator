using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using StockSimulator.GoogleFinanceDownloader;

namespace StockSimulator.Core
{
	/// <summary>
	/// Returns data for the symbol. If the symbol isn't in memory or doesn't
	/// exist, then we get it from the ol' internet thing.
	/// </summary>
	public class TickerDataStore
	{
		SortedDictionary<int, TickerData> _symbolsInMemory;

		/// <summary>
		/// Constructor
		/// </summary>
		public TickerDataStore()
		{
			_symbolsInMemory = new SortedDictionary<int, TickerData>();
		}

		/// <summary>
		/// Gets the symbol data from either memory, disk, or a server.
		/// </summary>
		/// <param name="ticker">Ticker to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the ticker</returns>
		public TickerData GetTickerData(TickerExchangePair ticker, DateTime start, DateTime end)
		{
			TickerData data = new TickerData(ticker);

			// The symbol exists in memory already.
			int key = ticker.GetHashCode();
			if (_symbolsInMemory.ContainsKey(key))
			{
				data = _symbolsInMemory[key];

				// Make sure the symbol has data for the dates that we want
				// If it doesn't then we have to get some more data.
				if (data.Start > start || data.End < end)
				{
					data = GetDataFromDiskOrServer(ticker, start, end);
				}

			}
			// Symbol isn't in memory so we need to load from the disk or the server.
			else
			{
				data = GetDataFromDiskOrServer(ticker, start, end);
			}
			return data;
		}

		/// <summary>
		/// Tries to get the data from the disk first. If all the data isn't on disk
		/// then request it from the server.
		/// </summary>
		/// <param name="ticker">Ticker to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the ticker</returns>
		private TickerData GetDataFromDiskOrServer(TickerExchangePair ticker, DateTime start, DateTime end)
		{
			TickerData data = GetDataFromDisk(ticker, start, end);

			// Check if the data from the disk is in the range. If it's
			// not then we need to get data from the server so we have it.
			if (data == null || data.Start > start || data.End < end)
			{
				try
				{
					data = GetDataFromServer(ticker, start, end);
					AppendNewData(ticker, data);

					// TODO: Save this new data to the disk so we don't have to query
					// the internet as much.
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}
			else
			{
				AppendNewData(ticker, data);
			}

			return data;
		}

		/// <summary>
		/// Appends the new data to the data in memory. Or if the data isn't in memory yet,
		/// it will add it to memory.
		/// </summary>
		/// <param name="ticker">Ticker to get data for</param>
		/// <param name="newData">Data to append</param>
		private void AppendNewData(TickerExchangePair ticker, TickerData newData)
		{
			int key = ticker.GetHashCode();
			if (_symbolsInMemory.ContainsKey(key))
			{
				TickerData existingData = _symbolsInMemory[key];

				// Need to check if the date range contains any gaps
				// For example, if they had a week of data from 3/1/2011 and they
				// tried to add a week from 2/1/2011. We need to get the data to stich
				// the two ranges together so that when we check if a particular date
				// exists, we don't have to worry about their being any gaps in that data.
				if (newData.End < existingData.Start)
				{
					TickerData stitch = GetDataFromServer(ticker, newData.End, existingData.Start);
					existingData.AppendData(stitch);
				}
				if (newData.Start > existingData.End)
				{
					TickerData stitch = GetDataFromServer(ticker, existingData.End, newData.Start);
					existingData.AppendData(stitch);
				}

				existingData.AppendData(newData);
			}
			else
			{
				_symbolsInMemory[key] = newData;
			}
		}

		/// <summary>
		/// Gets the data from the disk for the symbol dates requested. If it doesn't exist
		/// on disk, then we'll have to get it from the internet and then save it on disk
		/// for later use.
		/// </summary>
		/// <param name="ticker">Ticker to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the ticker</returns>
		private TickerData GetDataFromDisk(TickerExchangePair ticker, DateTime start, DateTime end)
		{
			// If the file doesn't exist then we for sure have to pull it from the internet.
			// If file != exist
			return null;

			// If file exists
			// Load it into memory
			// Make sure that we have all the dates we need
			// If not all dates


		}

		/// <summary>
		/// Gets the data from the webserver and saves it onto disk for later usage.
		/// </summary>
		/// <param name="ticker">ticker to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the ticker</returns>
		private TickerData GetDataFromServer(TickerExchangePair ticker, DateTime start, DateTime end)
		{
			string downloadedData;

			DownloadURIBuilder uriBuilder = new DownloadURIBuilder(ticker.Exchange, ticker.Ticker);
			
			// Need to always get up till today from the server since google only supports a start date.
			string uri = uriBuilder.getGetPricesUrlForRecentData(start, DateTime.Now);

			using (WebClient wClient = new WebClient())
			{
				downloadedData = wClient.DownloadString(uri);
			}

			using (MemoryStream ms = new MemoryStream(System.Text.Encoding.Default.GetBytes(downloadedData)))
			{
				DataProcessor dp = new DataProcessor();
				string errorMessage;
				string resultValue;

				resultValue = dp.processStreamMadeOfOneDayLinesToExtractHistoricalData(ms, out errorMessage);

				if (!string.IsNullOrEmpty(errorMessage))
				{
					throw new Exception(errorMessage);
				}
				else
				{
					return CreateTickerDataFromString(resultValue, ticker, start, end);
				}
			}
		}

		/// <summary>
		/// Creates an object of ticker data from the stream passed in.
		/// </summary>
		/// <param name="data">String of ticker data</param>
		/// <param name="start">Start date of the data</param>
		/// <param name="end">End date of the data</param>
		/// <returns>Returns an object created from the ticker data string</returns>
		private TickerData CreateTickerDataFromString(string data, TickerExchangePair ticker, DateTime start, DateTime end)
		{
			if (string.IsNullOrEmpty(data))
			{
				throw new Exception("No ticker data to parse.");
			}

			using (StringReader reader = new StringReader(data))
			{
				string line = string.Empty;

				// Strip off the headers if the are present
				if (reader.Peek() == 68) // "D"
				{
					reader.ReadLine();
				}

				TickerData tickerData = new TickerData(ticker, start, end);

				// Read each line of the string and convert it into numerical data and dates.
				do
				{
					line = reader.ReadLine();
					if (line != null)
					{
						string[] splitData = line.Split(new char[] { ',' });
						DateTime lineDate = DateTime.Parse(splitData[0]);
						
						// Because of the way google returns data, we don't always get our exact dates.
						// What we get is an interval of dates containing the ones we asked for, so 
						// we'll filter that data down to just the dates we want.
						if (lineDate < start)
						{
							continue;
						}
						if (lineDate > end)
						{
							break;
						}

						// Add the data to our object.
						tickerData.Dates.Add(lineDate);
						tickerData.Open.Add(Convert.ToDouble(splitData[1]));
						tickerData.High.Add(Convert.ToDouble(splitData[2]));
						tickerData.Low.Add(Convert.ToDouble(splitData[3]));
						tickerData.Close.Add(Convert.ToDouble(splitData[4]));
						tickerData.Volume.Add(Convert.ToInt64(splitData[5]));
						tickerData.NumBars = tickerData.Dates.Count;
					}
				} while (line != null);

				return tickerData;
			}
		}
	}
}
