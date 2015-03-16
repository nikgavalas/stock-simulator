using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using StockSimulator.GoogleFinanceDownloader;
using StockSimulator.YahooFinanceDownloader;

namespace StockSimulator.Core
{
	/// <summary>
	/// Returns data for the symbol. If the symbol isn't in memory or doesn't
	/// exist, then we get it from the ol' internet thing.
	/// </summary>
	public class TickerDataStore
	{
		public TickerData SimTickerDates { get; set; }

		private TickerData _allTickerDates { get; set; }

		private SortedDictionary<int, TickerData> _symbolsInMemory;

		private readonly string _cacheFolder = @"DataCache\";
		private readonly DateTime _earliestStartAllowed = new DateTime(2000, 1, 4); // Max allowed is the first trading day of the year.

		/// <summary>
		/// Constructor
		/// </summary>
		public TickerDataStore()
		{
			_symbolsInMemory = new SortedDictionary<int, TickerData>();

			// This holds all the valid trading days from our earliest allowed start date up to 
			// the latest requested date. This is so that if we get a symbol (ex Facebook) which 
			// doesn't have data from too long, but we still want to include it in the simulation
			// when it does have data. We just fill the facebook data with zeros using the dates
			// from this ticker. That way all the tickers have the same number of bars.
			_allTickerDates = new TickerData(new TickerExchangePair("NASDAQ", "INTC"));
			SimTickerDates = new TickerData(new TickerExchangePair("NASDAQ", "INTC"));

			Directory.CreateDirectory(_cacheFolder);
		}

		/// <summary>
		/// Deletes any files in the cache folder.
		/// </summary>
		public void ClearCache()
		{
			DirectoryInfo cacheInfo = new DirectoryInfo(_cacheFolder);
			foreach (FileInfo item in cacheInfo.GetFiles())
			{
				item.Delete();
			}
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
			// First thing is make sure we have a valid list of trading dates. Easiest way to do this is 
			// just download ticker data from the internet and use those dates. Calculating what days the
			// market actually traded isn't a super simple task.
			if (ticker.Ticker != _allTickerDates.TickerAndExchange.Ticker &&
				(_allTickerDates.Start > _earliestStartAllowed || _allTickerDates.End < end))
			{
				_allTickerDates = GetDataFromDiskOrServer(_allTickerDates.TickerAndExchange, _earliestStartAllowed, end);
				_allTickerDates.ZeroPrices();
			}

			// Save this so we have an accurate amount of tradeable bars along with the dates.
			if (SimTickerDates.Start != start || SimTickerDates.End != end)
			{
				SimTickerDates = _allTickerDates.SubSet(start, end);
			}

			TickerData data = new TickerData(ticker);

			// The symbol exists in memory already.
			int key = ticker.GetHashCode();
			if (_symbolsInMemory.ContainsKey(key))
			{
				TickerData inMemoryData = _symbolsInMemory[key];

				// Anything in memory should have data from our earliest date. If the stock wasn't around 
				// prior to that date then it will have 0's for the data instead.
				if (inMemoryData.Start > _earliestStartAllowed)
				{
					throw new Exception("The data stored in memory isn't filled to the earliest start date. Please restart the program!");
				}
				// We don't have all the data in memory past the end, so we need to get that data and append it.
				else if (end > inMemoryData.End)
				{
					data = GetDataFromDiskOrServer(ticker, _earliestStartAllowed, end);
					
					// Update the data in memory so it has it next time it runs.
					_symbolsInMemory[key] = data;

					// Return only the dates requested.
					data = data.SubSet(start, end);
				}
				// Not requesting everything that is in the memory. This is generally the case.
				else if (start > inMemoryData.Start || end < inMemoryData.End)
				{
					data = inMemoryData.SubSet(start, end);
				}
				// We wanted everything that is memory.
				else
				{
					data = inMemoryData;
				}
			}
			// Symbol isn't in memory so we need to load from the disk or the server.
			else
			{
				// Always start by loading everything we have our earliest date so that
				// anytime we eventually will have all the data saved allowing us to
				// test lots of different date ranges without having to hit the disk or internet.
				data = GetDataFromDiskOrServer(ticker, _earliestStartAllowed, end);

				if (data != null)
				{
					// Save in memory for next time.
					_symbolsInMemory[key] = data;

					data = data.SubSet(start, end);
				}
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
			TickerData data = GetDataFromDisk(ticker);
			bool diskDataNeedsUpdate = data == null || data.Start > start || data.End < end;

			// If the data is not on disk at all or there was a problem reading it, then 
			// we definitely get it from the server.
			if (diskDataNeedsUpdate)
			{
				try
				{
					Simulator.WriteMessage("[" + ticker.ToString() + "] Downloading data");
					data = GetDataFromGoogleServerAlt(ticker, start, end);

					// If there is not enough data from the server to fill our dates, then 
					// pad the rest of the dates with 0's.
					if (data.Start > start)
					{
						data.AppendData(_allTickerDates.SubSet(start, data.Start));
					}
					if (data.End < end)
					{
						data.AppendData(_allTickerDates.SubSet(data.End, end));
					}

					// Save the data so we can resuse it again without hitting the server.
					SaveTickerData(ticker, data);
				}
				catch (Exception e)
				{
					Simulator.WriteMessage("[" + ticker.ToString() + "] Error downloading and parsing data-Exception: " + e.Message);
				}
			}

			return data;
		}

		/// <summary>
		/// Saves all the ticker data to a file so it can be resused without us downloading from the server.
		/// </summary>
		/// <param name="ticker">Ticker exchange name</param>
		/// <param name="newData">Ticker data to save</param>
		private void SaveTickerData(TickerExchangePair ticker, TickerData newData)
		{
			string fileAndPath = GetTickerFilename(ticker);
			string contents = "Date,Open,High,Low,Close,Volume," + Environment.NewLine;
			contents += newData.ToString();
			try
			{
				File.WriteAllText(fileAndPath, contents);
			}
			catch (Exception e)
			{
				Simulator.WriteMessage("[" + ticker.ToString() + "] Save ticker exception: " + e.Message);
			}
		}
		
		/// <summary>
		/// Builds a cache file name for a ticker.
		/// </summary>
		/// <param name="ticker">Ticker exhange name</param>
		/// <returns>Filename to for the ticker</returns>
		private string GetTickerFilename(TickerExchangePair ticker)
		{
			return _cacheFolder + ticker.ToString() + ".csv";
		}


		/// <summary>
		/// Gets the data from the disk for the symbol dates requested. If it doesn't exist
		/// on disk, then we'll have to get it from the internet and then save it on disk
		/// for later use.
		/// </summary>
		/// <param name="ticker">Ticker to get data for</param>
		/// <returns>Data (price, volume, etc) for the ticker</returns>
		private TickerData GetDataFromDisk(TickerExchangePair ticker)
		{
			string fileAndPath = GetTickerFilename(ticker);

			// If the file doesn't exist then we for sure have to pull it from the internet later.
			if (File.Exists(fileAndPath))
			{
				try
				{
					Simulator.WriteMessage("[" + ticker.ToString() + "] Loading from disk data");

					StreamReader file = new StreamReader(fileAndPath);
					string line;
					StringBuilder sb = new StringBuilder();
					while ((line = file.ReadLine()) != null)
					{
						sb.AppendLine(line);
					}

					return CreateTickerDataFromString(sb.ToString(), ticker, new DateTime(1970, 1, 1), new DateTime(1970, 1, 1));
				}
				catch (Exception e)
				{
					Simulator.WriteMessage("[" + ticker.ToString() + "] Error reading and parsing file-Exception: " + e.Message);
				}
			}
		
			// If file != exist
			return null;
		}


		/// <summary>
		/// Gets the data from the webserver and saves it onto disk for later usage.
		/// </summary>
		/// <param name="ticker">ticker to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the ticker</returns>
		private TickerData GetDataFromGoogleServerAlt(TickerExchangePair ticker, DateTime start, DateTime end)
		{
			string downloadedData;

			string baseUrl = "http://www.google.com/finance/historical?q={0}&startdate={1}&enddate={2}&ei=803jVKPmEoryrAGH34CgDA&output=csv";
			string uri = string.Format(baseUrl,
				ticker.Ticker,
				start.ToString(@"MMM+d\%2C+yyyy"),
				end.ToString(@"MMM+d\%2C+yyyy")
			);

			using (WebClient wClient = new WebClient())
			{
				downloadedData = wClient.DownloadString(uri);
			}

			using (MemoryStream ms = new MemoryStream(System.Text.Encoding.Default.GetBytes(downloadedData)))
			{
				StreamReader sr = new StreamReader(ms);
				string line;
				List<string> lines = new List<string>();
				while ((line = sr.ReadLine()) != null)
				{
					lines.Add(line);
				}

				// Read all the lines from back to front and ignore the headers in the beginning of the file.
				StringBuilder sb = new StringBuilder();
				for (int i = lines.Count - 1; i > 0; i--)
				{
					sb.AppendLine(lines[i]);
				}

				string resultValue = sb.ToString();
				return CreateTickerDataFromString(resultValue, ticker, start, end);
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

				TickerData tickerData = new TickerData(ticker);

				// Value for an invalid date.
				DateTime invalidDate = new DateTime(1970, 1, 1);

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
						if (start != invalidDate && lineDate < start)
						{
							continue;
						}
						if (end != invalidDate && lineDate > end)
						{
							break;
						}

						// Add the data to our object.
						double open = 0;
						double high = 0;
						double low = 0;
						double close = 0;
						long volume = 0;
					
						if (IsDataFieldValid(splitData, 1) && IsDataFieldValid(splitData, 2) && IsDataFieldValid(splitData, 3) && IsDataFieldValid(splitData, 4) && IsDataFieldValid(splitData, 5))
						{
							open = Convert.ToDouble(splitData[1]);
							high = Convert.ToDouble(splitData[2]);
							low = Convert.ToDouble(splitData[3]);
							close = Convert.ToDouble(splitData[4]);
							volume = Convert.ToInt64(splitData[5]);
						}

						tickerData.Dates.Add(lineDate);
						tickerData.Open.Add(open);
						tickerData.High.Add(high);
						tickerData.Low.Add(low);
						tickerData.Close.Add(close);
						tickerData.Volume.Add(volume);
						tickerData.NumBars = tickerData.Dates.Count;

						// Extra non-downloaded data.
						tickerData.Typical.Add((high + low + close) / 3);
						tickerData.Median.Add((high + low) / 2);

						// Google has a weird bug where sometimes the open price will turn out to be
						// zero for some random bars. If this happens we'll just hack it now so that 
						// the open price = the close from the previous day.
						if (tickerData.Open[tickerData.Open.Count - 1] <= 0)
						{
							tickerData.Open[tickerData.Open.Count - 1] = (tickerData.Close.Count > 1) ? tickerData.Close[tickerData.Close.Count - 2] :
								tickerData.Close[tickerData.Close.Count - 1];
							//Simulator.WriteMessage("[" + ticker.ToString() + "] Open price 0 for bar: " + (tickerData.Open.Count - 1));
						}
						if (tickerData.Close[tickerData.Close.Count - 1] <= 0)
						{
							//Simulator.WriteMessage("[" + ticker.ToString() + "] Close price 0 for bar: " + (tickerData.Open.Count - 1));
						}
					}
				} while (line != null);

				tickerData.Start = tickerData.Dates[0];
				tickerData.End = tickerData.Dates[tickerData.Dates.Count - 1];

				return tickerData;
			}
		}

		/// <summary>
		/// Verifies that the datafield exists and has valid data.
		/// </summary>
		/// <param name="data">Array of datafields in the string</param>
		/// <param name="index">Index to verify</param>
		private bool IsDataFieldValid(string[] data, int index)
		{
			return index < data.Length && data[index] != "-";
		}

		/// <summary>
		/// Gets the data from the webserver and saves it onto disk for later usage.
		/// </summary>
		/// <param name="ticker">ticker to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the ticker</returns>
		//private TickerData GetDataFromGoogleServer(TickerExchangePair ticker, DateTime start, DateTime end)
		//{
		//	string downloadedData;

		//	DownloadURIBuilder uriBuilder = new DownloadURIBuilder(ticker.Exchange, ticker.Ticker);

		//	// Need to always get up till today from the server since google only supports a start date.
		//	string uri = uriBuilder.getGetPricesUrlForRecentData(start, DateTime.Now);

		//	using (WebClient wClient = new WebClient())
		//	{
		//		downloadedData = wClient.DownloadString(uri);
		//	}

		//	using (MemoryStream ms = new MemoryStream(System.Text.Encoding.Default.GetBytes(downloadedData)))
		//	{
		//		DataProcessor dp = new DataProcessor();
		//		string errorMessage;
		//		string resultValue;

		//		resultValue = dp.processStreamMadeOfOneDayLinesToExtractHistoricalData(ms, out errorMessage);

		//		if (!string.IsNullOrEmpty(errorMessage))
		//		{
		//			throw new Exception(errorMessage);
		//		}
		//		else
		//		{
		//			return CreateTickerDataFromString(resultValue, ticker, start, end);
		//		}
		//	}
		//}

		/// <summary>
		/// Gets the data from the webserver and saves it onto disk for later usage.
		/// </summary>
		/// <param name="ticker">ticker to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the ticker</returns>
		//private TickerData GetDataFromYahooServer(TickerExchangePair ticker, DateTime start, DateTime end)
		//{
		//	string downloadedData;

		//	YahooFinanceUriBuilder uriBuilder = new YahooFinanceUriBuilder();
		//	string uri = uriBuilder.GetDailyDataUrl(ticker.Ticker, start, end);

		//	using (WebClient wClient = new WebClient())
		//	{
		//		downloadedData = wClient.DownloadString(uri);
		//	}

		//	using (MemoryStream ms = new MemoryStream(System.Text.Encoding.Default.GetBytes(downloadedData)))
		//	{
		//		StreamReader sr = new StreamReader(ms);
		//		string line;
		//		List<string> lines = new List<string>();
		//		while ((line = sr.ReadLine()) != null)
		//		{
		//			lines.Add(line);
		//		}

		//		// Read all the lines from back to front and ignore the headers in the beginning of the file.
		//		StringBuilder sb = new StringBuilder();
		//		for (int i = lines.Count - 1; i > 0; i--)
		//		{
		//			sb.AppendLine(lines[i]);
		//		}

		//		string resultValue = sb.ToString();
		//		return CreateTickerDataFromString(resultValue, ticker, start, end);
		//	}
		//}


		/// <summary>
		/// Appends the new data to the data in memory. Or if the data isn't in memory yet,
		/// it will add it to memory.
		/// </summary>
		/// <param name="ticker">Ticker to get data for</param>
		/// <param name="newData">Data to append</param>
		//private void AppendNewData(TickerExchangePair ticker, TickerData newData)
		//{
		//	int key = ticker.GetHashCode();
		//	if (_symbolsInMemory.ContainsKey(key))
		//	{
		//		TickerData existingData = _symbolsInMemory[key];

		//		// Need to check if the date range contains any gaps
		//		// For example, if they had a week of data from 3/1/2011 and they
		//		// tried to add a week from 2/1/2011. We need to get the data to stich
		//		// the two ranges together so that when we check if a particular date
		//		// exists, we don't have to worry about their being any gaps in that data.
		//		if (newData.End < existingData.Start)
		//		{
		//			TickerData stitch = GetDataFromGoogleServerAlt(ticker, newData.End, existingData.Start);
		//			existingData.AppendData(stitch);
		//		}
		//		if (newData.Start > existingData.End)
		//		{
		//			TickerData stitch = GetDataFromGoogleServerAlt(ticker, existingData.End, newData.Start);
		//			existingData.AppendData(stitch);
		//		}

		//		existingData.AppendData(newData);
		//	}
		//	else
		//	{
		//		_symbolsInMemory[key] = newData;
		//	}
		//}
	
	
	}
}
