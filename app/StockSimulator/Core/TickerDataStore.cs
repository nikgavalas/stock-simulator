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
		SortedDictionary<int, TickerData> _symbolsInMemory;

		private readonly string _cacheFolder = @"DataCache\";

		/// <summary>
		/// Constructor
		/// </summary>
		public TickerDataStore()
		{
			_symbolsInMemory = new SortedDictionary<int, TickerData>();

			Directory.CreateDirectory(_cacheFolder);
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
				TickerData inMemoryData = _symbolsInMemory[key];

				// Make sure the symbol has data for the dates that we want
				// If it doesn't then we have to get some more data.
				if (start < inMemoryData.Start || end > inMemoryData.End)
				{
					data = GetDataFromDiskOrServer(ticker, start, end);
				}
				// Not requesting everything that is in the memory.
				else if (start > inMemoryData.Start || end < inMemoryData.End)
				{
					data = inMemoryData.SubSet(start, end);
				}
				// Otherwise the data is the same and we just return it.
				else
				{
					data = inMemoryData;
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
			if (data == null || start < data.Start || end > data.End)
			{
				try
				{
					TickerData serverData = GetDataFromGoogleServerAlt(ticker, start, end);
					AppendNewData(ticker, serverData);

					// Save the data so we can resuse it again without hitting the server.
					if (data != null)
					{
						data.AppendData(serverData);
					}
					else
					{
						data = serverData;
					}

					SaveTickerData(ticker, data);
					data = serverData;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}
			else
			{
				AppendNewData(ticker, data);
				data = data.SubSet(start, end);
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
				Console.WriteLine(e.Message);
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
					TickerData stitch = GetDataFromGoogleServerAlt(ticker, newData.End, existingData.Start);
					existingData.AppendData(stitch);
				}
				if (newData.Start > existingData.End)
				{
					TickerData stitch = GetDataFromGoogleServerAlt(ticker, existingData.End, newData.Start);
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
			string fileAndPath = GetTickerFilename(ticker);

			// If the file doesn't exist then we for sure have to pull it from the internet.
			if (File.Exists(fileAndPath))
			{
				try
				{
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
					Console.WriteLine(e.Message);
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
		private TickerData GetDataFromGoogleServer(TickerExchangePair ticker, DateTime start, DateTime end)
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
		/// Gets the data from the webserver and saves it onto disk for later usage.
		/// </summary>
		/// <param name="ticker">ticker to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the ticker</returns>
		private TickerData GetDataFromYahooServer(TickerExchangePair ticker, DateTime start, DateTime end)
		{
			string downloadedData;

			YahooFinanceUriBuilder uriBuilder = new YahooFinanceUriBuilder();
			string uri = uriBuilder.GetDailyDataUrl(ticker.Ticker, start, end);

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
		/// Gets the data from the webserver and saves it onto disk for later usage.
		/// </summary>
		/// <param name="ticker">ticker to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the ticker</returns>
		private TickerData GetDataFromGoogleServerAlt(TickerExchangePair ticker, DateTime start, DateTime end)
		{
			string downloadedData;

			string baseUrl = "http://www.google.com/finance/historical?output=csv&q={0}&startdate={1}&enddate={2}";
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
						double open = Convert.ToDouble(splitData[1]);
						double high = Convert.ToDouble(splitData[2]);
						double low = Convert.ToDouble(splitData[3]);
						double close = Convert.ToDouble(splitData[4]);

						tickerData.Dates.Add(lineDate);
						tickerData.Open.Add(open);
						tickerData.High.Add(high);
						tickerData.Low.Add(low);
						tickerData.Close.Add(close);
						tickerData.Volume.Add(Convert.ToInt64(splitData[5]));
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
							Console.WriteLine(ticker.ToString() + ": Open price 0 for bar: " + (tickerData.Open.Count - 1));
						}
						if (tickerData.Close[tickerData.Close.Count - 1] <= 0)
						{
							throw new Exception(ticker.ToString() + ": Open price 0 for bar: " + (tickerData.Open.Count - 1));
						}
					}
				} while (line != null);

				tickerData.Start = tickerData.Dates[0];
				tickerData.End = tickerData.Dates[tickerData.Dates.Count - 1];

				return tickerData;
			}
		}
	}
}
