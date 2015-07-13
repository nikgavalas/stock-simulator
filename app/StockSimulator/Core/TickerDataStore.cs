﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using StockSimulator.GoogleFinanceDownloader;
using StockSimulator.YahooFinanceDownloader;
using StockSimulator.Indicators;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace StockSimulator.Core
{
	/// <summary>
	/// Returns data for the symbol. If the symbol isn't in memory or doesn't
	/// exist, then we get it from the ol' internet thing.
	/// </summary>
	public class TickerDataStore
	{
		public SortedDictionary<DateTime, bool> SimTickerDates { get; set; }
		public string DataType { get; set; }

		private ConcurrentDictionary<int, TickerData> _symbolsInMemory;

		private readonly string _cacheFolder = @"DataCache\";
		private readonly DateTime _earliestStartAllowed = new DateTime(2000, 1, 4); // Max allowed is the first trading day of the year.

		private object _dateLock = new object();

		private enum DataFields
		{
			Date = 0,
			Open,
			High,
			Low,
			Close,
			Volume,
			Typical,
			Median,
			HigherState
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public TickerDataStore()
		{
			_symbolsInMemory = new ConcurrentDictionary<int, TickerData>();
			SimTickerDates = new SortedDictionary<DateTime, bool>();

			// Create the root datacache folders and all the sub folders for the different type
			// of data we support running with (daily, minute, 5 minute).
			Directory.CreateDirectory(_cacheFolder);
			SimulatorConfig.DataItemsSource dataTypes = new SimulatorConfig.DataItemsSource();
			Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection dataTypesValues = dataTypes.GetValues();
			for (int i = 0; i < dataTypesValues.Count; i++)
			{
				Directory.CreateDirectory(_cacheFolder + @"\" + dataTypesValues[i].Value);
			}
		}

		/// <summary>
		/// Deletes any files in the cache folder.
		/// </summary>
		public void ClearCache()
		{
			DirectoryInfo cacheInfo = new DirectoryInfo(_cacheFolder);
			foreach (DirectoryInfo item in cacheInfo.GetDirectories())
			{
				item.Delete(true);
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
			TickerData data = new TickerData(ticker);

			// The symbol exists in memory already.
			int key = ticker.GetHashCode();
			if (_symbolsInMemory.ContainsKey(key))
			{
				TickerData inMemoryData = _symbolsInMemory[key];

				// We don't have all the data in memory past the end, so we need to get that data and append it.
				if (end > inMemoryData.End || start < inMemoryData.Start)
				{
					data = GetDataFromDiskOrServer(ticker, start, end);
					
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
				data = GetDataFromDiskOrServer(ticker, start, end);

				if (data != null)
				{
					// Save in memory for next time.
					_symbolsInMemory[key] = data;

					data = data.SubSet(start, end);
				}
			}

			if (data != null)
			{
				// Save all the dates that this ticker has so that we have a list of dates that we can 
				// iterate through for trading periods. This is because each ticker can potentially have
				// different trading dates but for the main sim we want to go through all dates and if
				// the ticker has data for that time, we'll use it.
				lock (_dateLock)
				{
					for (int i = 0; i < data.Dates.Count; i++)
					{
						if (SimTickerDates.ContainsKey(data.Dates[i]) == false)
						{
							SimTickerDates[data.Dates[i]] = true;
						}
					}
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
			DateTime fileStartDate;
			DateTime fileEndDate;
			TickerData data = GetDataFromDisk(ticker, out fileStartDate, out fileEndDate);
			bool diskDataNeedsUpdate = data == null || fileStartDate > start || fileEndDate < end;

			// If the data is not on disk at all or there was a problem reading it, then 
			// we definitely get it from the server.
			if (diskDataNeedsUpdate)
			{
				try
				{
					Simulator.WriteMessage("[" + ticker.ToString() + "] Downloading data");
					TickerData serverData;
					if (Simulator.Config.DataType == "daily")
					{
						serverData = GetDataFromGoogleServerAlt(ticker, start, end);
					}
					else
					{
						int interval = 60;
						if (Simulator.Config.DataType == "fiveminute")
						{
							interval = 300;
						}
						else if (Simulator.Config.DataType == "threeminute")
						{
							interval = 180;
						}
						else if (Simulator.Config.DataType == "twominute")
						{
							interval = 120;
						}

						serverData = GetIntraDayDataFromGoogleServer(ticker, start, end, interval);
					}

					// Anytime we have to download data from the server always save the entire
					// data set. This is because the data is split adjusted so the data from
					// yesteray may not match the data from today.
					data = serverData;

					// Save the data so we can resuse it again without hitting the server.
					SaveTickerData(ticker, data, start, end);
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
		/// <param name="start">Start date requested</param>
		/// <param name="end">End date requested</param>
		private void SaveTickerData(TickerExchangePair ticker, TickerData newData, DateTime start, DateTime end)
		{
			string fileAndPath = GetTickerFilename(ticker);
			string contents = UtilityMethods.UnixTicks(start) + "," + UtilityMethods.UnixTicks(end) + "," + Environment.NewLine;
			contents += "Date,Open,High,Low,Close,Volume,Typical,Median,HigherState" + Environment.NewLine;
			contents += newData.WriteToString();
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
			return _cacheFolder + @"\" + Simulator.Config.DataType + @"\" + ticker.ToString() + ".csv";
		}


		/// <summary>
		/// Gets the data from the disk for the symbol dates requested. If it doesn't exist
		/// on disk, then we'll have to get it from the internet and then save it on disk
		/// for later use.
		/// </summary>
		/// <param name="ticker">Ticker to get data for</param>
		/// <param name="fileStartDate">The date that was the start of the requested date range. This can differ from the actual date that the server returns as the first date</param>
		/// <param name="fileEndDate">The date that was the end of the requested date range</param>
		/// <returns>Data (price, volume, etc) for the ticker</returns>
		private TickerData GetDataFromDisk(TickerExchangePair ticker, out DateTime fileStartDate, out DateTime fileEndDate)
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

					// The first line should be the saved start and end dates. Don't include that in the string builder
					// since it's only important for the callee of this function.
					line = file.ReadLine();
					string[] dates = line.Split(',');
					fileStartDate = UtilityMethods.ConvertFromUnixTimestamp(dates[0]);
					fileEndDate = UtilityMethods.ConvertFromUnixTimestamp(dates[1]);

					while ((line = file.ReadLine()) != null)
					{
						sb.AppendLine(line);
					}

					file.Close();

					return CreateTickerDataFromString(sb.ToString(), ticker, true, new DateTime(1970, 1, 1), new DateTime(1970, 1, 1));
				}
				catch (Exception e)
				{
					Simulator.WriteMessage("[" + ticker.ToString() + "] Error reading and parsing file-Exception: " + e.Message);
				}
			}
		
			// If file != exist
			fileStartDate = DateTime.Now;
			fileEndDate = DateTime.Now;
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
				return CreateTickerDataFromString(resultValue, ticker, false, start, end);
			}
		}

		/// <summary>
		/// Gets the data from the webserver and saves it onto disk for later usage.
		/// </summary>
		/// <param name="ticker">ticker to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <param name="interval">Interval in seconds of the data to retreive</param>
		/// <returns>Data (price, volume, etc) for the ticker</returns>
		private TickerData GetIntraDayDataFromGoogleServer(TickerExchangePair ticker, DateTime start, DateTime end, int interval)
		{
			string downloadedData;

			DownloadURIBuilder uriBuilder = new DownloadURIBuilder(ticker.Exchange, ticker.Ticker);

			// Need to always get up till today from the server since google only supports a start date.
			string uri = uriBuilder.getGetPricesUrlForIntraday(start, end, interval);

			using (WebClient wClient = new WebClient())
			{
				downloadedData = wClient.DownloadString(uri);
			}

			using (MemoryStream ms = new MemoryStream(System.Text.Encoding.Default.GetBytes(downloadedData)))
			{
				DataProcessor dp = new DataProcessor();
				string errorMessage;
				string resultValue;

				resultValue = dp.processIntradayStream(ms, out errorMessage);

				if (!string.IsNullOrEmpty(errorMessage))
				{
					throw new Exception(errorMessage);
				}
				else
				{
					return CreateTickerDataFromString(resultValue, ticker, false, start, end);
				}
			}
		}

		/// <summary>
		/// Creates an object of ticker data from the stream passed in.
		/// </summary>
		/// <param name="data">String of ticker data</param>
		/// <param name="ticker">Ticker name for this data string</param>
		/// <param name="isFromDisk">True if this string is loaded from our disk, saves a lot of calculation time</param>
		/// <param name="start">Start date of the data</param>
		/// <param name="end">End date of the data</param>
		/// <returns>Returns an object created from the ticker data string</returns>
		private TickerData CreateTickerDataFromString(string data, TickerExchangePair ticker, bool isFromDisk, DateTime start, DateTime end)
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
						DateTime lineDate = DateTime.MaxValue;
						long lineDateDigit = 0;
						if (long.TryParse(splitData[0], out lineDateDigit))
						{
							lineDate = UtilityMethods.ConvertFromUnixTimestamp(splitData[0]);
						}
						else
						{
							lineDate = DateTime.Parse(splitData[0]);
						}
						
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

						// Google has a weird bug where sometimes the open price will turn out to be
						// zero for some random bars. If this happens we'll just hack it now so that 
						// the open price = the close from the previous day.
						if (tickerData.Open[tickerData.Open.Count - 1] <= 0)
						{
							tickerData.Open[tickerData.Open.Count - 1] = (tickerData.Close.Count > 1) 
								? tickerData.Close[tickerData.Close.Count - 2]
								: tickerData.Close[tickerData.Close.Count - 1];
							//Simulator.WriteMessage("[" + ticker.ToString() + "] Open price 0 for bar: " + (tickerData.Open.Count - 1));
						}
						if (tickerData.Close[tickerData.Close.Count - 1] <= 0)
						{
							//Simulator.WriteMessage("[" + ticker.ToString() + "] Close price 0 for bar: " + (tickerData.Open.Count - 1));
						}

						// If this data is from the disk we don't need to calculate the extra fields since
						// we've already saved them in the file.
						if (isFromDisk)
						{
							tickerData.Typical.Add(Convert.ToDouble(splitData[(int)DataFields.Typical]));
							tickerData.Median.Add(Convert.ToDouble(splitData[(int)DataFields.Median]));
							tickerData.HigherTimeframeTrend.Add(Convert.ToDouble(splitData[(int)DataFields.HigherState]));
						}
						else
						{
							// Extra non-downloaded data.
							high = tickerData.High[tickerData.NumBars - 1];
							low = tickerData.Low[tickerData.NumBars - 1];
							close = tickerData.Close[tickerData.NumBars - 1];
							tickerData.Typical.Add((high + low + close) / 3);
							tickerData.Median.Add((high + low) / 2);

							// Calculate the higher momentum state for this bar. This is a pretty
							// time consuming function since it has to loop back through all the
							// bars before (and including) this one.
							double lastValue = tickerData.HigherTimeframeTrend.Count > 0 ? tickerData.HigherTimeframeTrend[tickerData.HigherTimeframeTrend.Count - 1] : Order.OrderType.Long;
							tickerData.HigherTimeframeTrend.Add(GetHigherTimerframeMomentumState(tickerData, lastValue));
						}
					}
				} while (line != null);

				tickerData.Start = tickerData.Dates[0];
				tickerData.End = tickerData.Dates[tickerData.Dates.Count - 1];
				tickerData.SaveDates();

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
		/// Returns what type of orders are allowed for this bar based on the
		/// higher time frame momentum analysis.
		/// </summary>
		/// <param name="ticker">Ticker data</param>
		/// <param name="lastState">Last state of the higher timeframe</param>
		/// <returns>Order type allowed for the last bar of the ticker data</returns>
		private double GetHigherTimerframeMomentumState(TickerData ticker, double lastState)
		{
			// Get all the bars for the higher timeframe.
			TickerData higherTickerData = GetHigherTimeframeBars(ticker);

			// Run the indicator and save it.
			//BressertDss higherTimeframeIndicator = new BressertDss(higherTickerData, new RunnableFactory(higherTickerData), 5);
			//Rsi3m3 higherTimeframeIndicator = new Rsi3m3(higherTickerData, new RunnableFactory(higherTickerData));
			//Macd higherTimeframeIndicator = new Macd(higherTickerData, new RunnableFactory(higherTickerData));
			DtOscillator higherTimeframeIndicator = new DtOscillator(higherTickerData) { PeriodRsi = 8, PeriodStoch = 5, PeriodSK = 3, PeriodSD = 3 };
			higherTimeframeIndicator.Initialize();
			higherTimeframeIndicator.RunToBar(higherTickerData.NumBars - 1);
			higherTimeframeIndicator.Shutdown();

			// Return what kind orders are allowed.
			double state = GetHigherTimeframeStateFromIndicator(higherTimeframeIndicator, higherTimeframeIndicator.Data.NumBars - 1, lastState);

			////////////////// START HIGHER TIME FRAME DEBUGGING ////////////////////
			if (Simulator.Config.OutputHigherTimeframeData)
			{
				DateTime outputDate = higherTickerData.Dates[higherTickerData.Dates.Count - 1];
				List<double> states = new List<double>(ticker.HigherTimeframeTrend);
				states.Add(state);
				Simulator.DataOutput.OutputHigherTimeframeData(
					outputDate,
					higherTimeframeIndicator,
					higherTickerData,
					ticker,
					states);
			}
			//////////////////  END  HIGHER TIME FRAME DEBUGGING ////////////////////

			return state;
		}

		/// <summary>
		/// Returns aggregated bars for the higher timeframe from the lower time frame data.
		/// </summary>
		/// <param name="ticker">Lower timeframe ticker data</param>
		/// <returns>Higher timeframe ticker data</returns>
		private TickerData GetHigherTimeframeBars(TickerData ticker)
		{
			double open = 0;
			double high = 0;
			double low = 0;
			double close = 0;
			long volume = 0;
			int barCount = 0;

			// Reset the states since we'll calculate them again.
			TickerData higherData = new TickerData(ticker.TickerAndExchange);

			// Aggregate all the data into the higher timeframe.
			for (int i = 0; i < ticker.Dates.Count; i++)
			{
				// The first bar open we'll treat as the open price and set the high and low.
				// Volume gets reset as it's cumulative through all the bars.
				if (barCount == 0)
				{
					open = ticker.Open[i];
					low = ticker.Low[i];
					high = ticker.High[i];
					volume = 0;
				}

				// If this low is lower than the saved low, we have a new low. 
				// Same for high but opposite of course.
				if (ticker.Low[i] < low)
				{
					low = ticker.Low[i];
				}
				if (ticker.High[i] > high)
				{
					high = ticker.High[i];
				}

				// Move to the next bar to aggregate from.
				++barCount;
				volume += ticker.Volume[i];

				// The last bar close is treated as the close. Now it's time to save all
				// the aggregated data as one bar for the higher timeframe.
				// We also want to do this if the for loop is just about to exit. We may not
				// have the number of bars we wanted for the aggregate, but we want to at least have
				// something for the last bar. Ex. We have 5 bars set for the higher timeframe length,
				// but we've only got 3 bars of data and the for loop will end on the next iteration.
				// In that case we want to use the 3 bars we have for the data.
				if (barCount == Simulator.Config.NumBarsHigherTimeframe || (i + 1) == ticker.Dates.Count)
				{
					close = ticker.Close[i];

					higherData.Dates.Add(ticker.Dates[i]); // Use the ending aggregated date as the date for the higher timeframe.
					higherData.Open.Add(open);
					higherData.High.Add(high);
					higherData.Low.Add(low);
					higherData.Close.Add(close);
					higherData.Volume.Add(volume);
					higherData.NumBars = higherData.Dates.Count;

					// Start aggregating a new set.
					barCount = 0;
				}
			}

			return higherData;
		}

		/// <summary>
		/// Returns the state of the higher momentum bar. Any momentum indicator can be used here.
		/// </summary>
		/// <param name="indicator">Momentum indicator to use</param>
		/// <param name="curBar">Current bar in the momentum simulation</param>
		/// <param name="lastState">Last state of the higher timeframe</param>
		/// <returns>The state of the higher momentum indicator</returns>
		//private double GetHigherTimeframeStateFromIndicator(BressertDss bressertDss, int currentBar)
		//private double GetHigherTimeframeStateFromIndicator(Rsi3m3 rsi, int currentBar, double lastState)
		//{
		//	if (currentBar >= 2)
		//	{
		//		if (DataSeries.IsBelow(rsi.Value, 30, currentBar, 2) != -1 && UtilityMethods.IsValley(rsi.Value, currentBar))
		//		{
		//			return Order.OrderType.Long;
		//		}
		//		if (DataSeries.IsAbove(rsi.Value, 70, currentBar, 2) != -1 && UtilityMethods.IsPeak(rsi.Value, currentBar))
		//		{
		//			return Order.OrderType.Short;
		//		}
		//	}

		//	return lastState;
		//}

		//private double GetHigherTimeframeStateFromIndicator(BressertDss bressertDss, int currentBar, double lastState)
		//{
		//	if (currentBar >= 2)
		//	{
		//		if (DataSeries.IsBelow(bressertDss.Value, 40, currentBar, 2) != -1 && UtilityMethods.IsValley(bressertDss.Value, currentBar))
		//		{
		//			return Order.OrderType.Long;
		//		}
		//		if (DataSeries.IsAbove(bressertDss.Value, 60, currentBar, 2) != -1 && UtilityMethods.IsPeak(bressertDss.Value, currentBar))
		//		{
		//			return Order.OrderType.Short;
		//		}
		//	}

		//	return lastState;
		//}

		//private double GetHigherTimeframeStateFromIndicator(Macd indicator, int currentBar, double lastState)
		//{
		//	return DataSeries.IsAbove(indicator.Value, indicator.Avg, currentBar, 0) != -1 ||
		//		indicator.Value[currentBar] == indicator.Avg[currentBar] ? Order.OrderType.Long : Order.OrderType.Short;
		//}

		private double GetHigherTimeframeStateFromIndicator(DtOscillator ind, int currentBar, double lastState)
		{
			if (currentBar > 2)
			{
				if (ind.SK[currentBar] <= 50.0 && UtilityMethods.IsValley(ind.SK, currentBar))
				{
					return Order.OrderType.Long;
				}
				else if (ind.SK[currentBar] >= 50.0 && UtilityMethods.IsPeak(ind.SK, currentBar))
				{
					return Order.OrderType.Short;
				}
				//if (DataSeries.CrossAbove(ind.SK, ind.SD, currentBar, 0) != -1)
				//{
				//	return Order.OrderType.Long;
				//}
				//else if (DataSeries.CrossBelow(ind.SK, ind.SD, currentBar, 0) != -1)
				//{
				//	return Order.OrderType.Short;
				//}

				//if (DataSeries.IsBelow(ind.SK, 25, currentBar, 1) != -1)
				//{
				//	if (DataSeries.CrossAbove(ind.SK, ind.SD, currentBar, 0) != -1)
				//	{
				//		return Order.OrderType.Long;
				//	}
				//}
				//else if (DataSeries.IsAbove(ind.SK, 75, currentBar, 1) != -1)
				//{
				//	if (DataSeries.CrossBelow(ind.SK, ind.SD, currentBar, 0) != -1)
				//	{
				//		return Order.OrderType.Short;
				//	}
				//}
			}

			return lastState;
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
