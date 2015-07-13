using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StockSimulator.Strategies;
using StockSimulator.Core.JsonConverters;
using System.Text.RegularExpressions;

namespace StockSimulator.Core
{
	/// <summary>
	/// Handles storing data for output and outputting it to json
	/// </summary>
	public class DataOutputter
	{
		private class JsonBuyList
		{
			[JsonProperty("ticker")]
			public string Ticker { get; set; }

			[JsonProperty("orderType")]
			public string OrderType { get; set; }

			[JsonProperty("strategyName")]
			public string StrategyName { get; set; }

			[JsonProperty("percent")]
			public double Percent { get; set; }

			public JsonBuyList(string ticker, string orderType, string strategyName, double percent)
			{
				Ticker = ticker;
				OrderType = orderType;
				StrategyName = strategyName;
				Percent = percent;
			}
		}

		private class JsonOrderState
		{
			[JsonProperty("dates")]
			public List<DateTime> Dates { get; set; }

			[JsonProperty("states")]
			public List<double> States { get; set; }

			public JsonOrderState(List<DateTime> _dates, List<double> _states)
			{
				Dates = _dates;
				States = _states;
			}
		}

		private Dictionary<int, TickerData> _tickerData;

		private Dictionary<int, Indicator> _indicators;

		private Dictionary<DateTime, List<JsonBuyList>> _buyLists;

		private string _outputFolder;
		private string _timeString;
		private object tickerLock;
		private object indicatorLock;

		/// <summary>
		/// Constructor
		/// </summary>
		public DataOutputter()
		{
			_tickerData = new Dictionary<int, TickerData>();
			_indicators = new Dictionary<int, Indicator>();
			_buyLists = new Dictionary<DateTime, List<JsonBuyList>>();
			tickerLock = new object();
			indicatorLock = new object();

			_timeString = DateTime.Now.ToString("MM-dd-yyyyTHH-mm-ss-ffff");
			_outputFolder = GetOutputFolder();
			Directory.CreateDirectory(_outputFolder.TrimEnd('\\'));
		}

		/// <summary>
		/// Saves the ticker data for outputting later.
		/// </summary>
		/// <param name="data">Ticker data to save</param>
		public void SaveTickerData(TickerData data)
		{
			lock (tickerLock)
			{
				int key = data.TickerAndExchange.GetHashCode();
				if (!_tickerData.ContainsKey(key))
				{
					_tickerData[key] = data;
				}
			}
		}

		/// <summary>
		/// Saves the indicator data so it can be outtputed at the end.
		/// </summary>
		/// <param name="indicator">The indicator to save</param>
		public void SaveIndicator(Indicator indicator)
		{
			lock (indicatorLock)
			{
				int key = (indicator.Data.TickerAndExchange.ToString() + indicator.ToString()).GetHashCode();
				if (!_indicators.ContainsKey(key))
				{
					_indicators[key] = indicator;
				}
			}
		}

		/// <summary>
		/// Outputs the buy list for the current bar.
		/// </summary>
		/// <param name="buyList">List of stocks that are displaying a reason to buy</param>
		/// <param name="currentDate">The date to use as the output index</param>
		public void SaveBuyList(List<BestOfRootStrategies> buyList, DateTime currentDate)
		{
			// Convert the buy list into better serializable data.
			_buyLists[currentDate] = new List<JsonBuyList>();
			List<JsonBuyList> outputList = _buyLists[currentDate];
			for (int i = 0; i < buyList.Count; i++)
			{
				outputList.Add(new JsonBuyList(buyList[i].Data.TickerAndExchange.ToString(),
					buyList[i].Bars[buyList[i].Data.GetBar(currentDate)].StrategyOrderType == Order.OrderType.Long ? "Long" : "Short",
					buyList[i].Bars[buyList[i].Data.GetBar(currentDate)].HighestStrategyName,
					buyList[i].Bars[buyList[i].Data.GetBar(currentDate)].HighestPercent
				));
			}
		}

		/// <summary>
		/// Outputs the data to a folder with the time the simulator finished.
		/// </summary>
		/// <returns>The folder the data was outtputed to</returns>
		public string OutputData()
		{
			OutputConfigFile();
			OutputBuyList();
			OutputPriceData();
			//OutputIndicatorData();
			OutputTickerStats();
			OutputStrategyStats();
			OutputMainStrategy();

			return _timeString;
		}

		/// <summary>
		/// Outputs all the higher time frame debug output for a date.
		/// </summary>
		/// <param name="date">Date to ouput</param>
		/// <param name="ind">Indicator data used for the higher timeframe calculations</param>
		/// <param name="higherData">Higher timeframe bar data</param>
		/// <param name="lowerData">Lower timeframe bar data</param>
		/// <param name="states">Order type states for each lower timeframe bar up to this date</param>
		public void OutputHigherTimeframeData(DateTime date, Indicator ind, TickerData higherData, TickerData lowerData, List<double> states)
		{
			string rootFolderName = Simulator.Config.OutputFolder + "\\higher\\" + lowerData.TickerAndExchange.ToString() + "\\";
			Directory.CreateDirectory(rootFolderName);

			ind.Serialize(ind.Data.NumBars - 1);
			higherData.PrepareForSerialization();

			string folderName = rootFolderName + date.ToString("yyyy-MM-dd");

			string jsonOutput = JsonConvert.SerializeObject(higherData);
			string filename = folderName + "-data.json";
			File.WriteAllText(filename, jsonOutput);

			jsonOutput = JsonConvert.SerializeObject(ind);
			filename = folderName + "-ind.json";
			File.WriteAllText(filename, jsonOutput);

			JsonOrderState orderState = new JsonOrderState(lowerData.Dates, states);
			jsonOutput = JsonConvert.SerializeObject(orderState);
			filename = folderName + "-states.json";
			File.WriteAllText(filename, jsonOutput);

			ind.FreeResourcesAfterSerialization();
			higherData.FreeResourcesAfterSerialization();
		}

		/// <summary>
		/// Uses the date string as a subfolder for the output.
		/// </summary>
		/// <returns>Root folder for the output with the date as the subfolder</returns>
		private string GetOutputFolder()
		{
			return Simulator.Config.OutputFolder + "\\" + _timeString + "\\";
		}

		/// <summary>
		/// Returns if this string is a valid filename string or not.
		/// </summary>
		/// <param name="testName">Filename</param>
		/// <returns>See summary</returns>
		bool IsValidFilename(string testName)
		{
			Regex containsABadCharacter = new Regex("["
						+ Regex.Escape(new string(System.IO.Path.GetInvalidPathChars())) + "]");
			if (containsABadCharacter.IsMatch(testName)) 
			{
				return false; 
			}

			// other checks for UNC, drive-path format, etc
			if (testName.Contains('*'))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Outputs info about how each ticker has performed.
		/// </summary>
		private void OutputTickerStats()
		{
			List<StrategyStatistics> allTickerStatistics = new List<StrategyStatistics>();

			foreach (KeyValuePair<int, List<Order>> tickerOrder in Simulator.Orders.TickerDictionary)
			{
				List<Order> orders = tickerOrder.Value;
				if (orders.Count > 0)
				{
					string tickerName = orders[0].Ticker.TickerAndExchange.ToString();
					StrategyStatistics tickerStats = new StrategyStatistics(tickerName, Order.OrderType.Long);

					// Catagorize and total all the orders by ticker.
					for (int i = 0; i < orders.Count; i++)
					{
						// Get the strategy name but skip the main strategy as it gets process differently.
						string strategyName = orders[i].StrategyName;
						if (strategyName == "MainStrategy")
						{
							continue;
						}

						tickerStats.AddOrder(orders[i]);
					}

					tickerStats.CalculateStatistics();
					allTickerStatistics.Add(tickerStats);

					//jsonOutput = JsonConvert.SerializeObject(tickerStats, Formatting.Indented);
					//folderName = _outputFolder + "ticker-statistics\\" + tickerName;
					//filename = folderName + "\\overall.json";
					//Directory.CreateDirectory(folderName);
					//File.WriteAllText(filename, jsonOutput);


				}
			}

			allTickerStatistics.Sort((a, b) => b.Gain.CompareTo(a.Gain));
			string jsonOutput = JsonConvert.SerializeObject(allTickerStatistics);
			string filename = _outputFolder + "overall-tickers.json";
			File.WriteAllText(filename, jsonOutput);

			// Output a csv of all the stocks with positive gains in case we want to 
			// use that file as an input to do even better.
			allTickerStatistics.RemoveAll(a => a.Gain <= 0);
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < allTickerStatistics.Count; i++)
			{
				string[] split = allTickerStatistics[i].StrategyName.Split('-');
				sb.Append(split[0] + "," + split[1]);

				if (i + 1 < allTickerStatistics.Count)
				{
					sb.Append("," + Environment.NewLine);
				}
			}
			filename = _outputFolder + "positive-gainers.csv";
			File.WriteAllText(filename, sb.ToString());
		}

		/// <summary>
		/// Outputs a buy list for each day or just the last day with the option set in the config.
		/// </summary>
		void OutputBuyList()
		{
			string jsonOutput;
			string filename;

			// Create the buy list directory to hold all the buy list files.
			string folderName = _outputFolder + "buylist\\";
			Directory.CreateDirectory(folderName);

			// Only output the list for the last day simulated.
			if (Simulator.Config.OnlyOutputLastBuyList == true)
			{
				KeyValuePair<DateTime, List<JsonBuyList>> buyList = _buyLists.Last();
				jsonOutput = JsonConvert.SerializeObject(buyList.Value, Formatting.Indented);
				filename = folderName + buyList.Key.ToString("yyyy-MM-dd") + ".json";
				File.WriteAllText(filename, jsonOutput);				
			}
			// Output the lists for every day.
			else
			{
				foreach (KeyValuePair<DateTime, List<JsonBuyList>> buyList in _buyLists)
				{
					jsonOutput = JsonConvert.SerializeObject(buyList.Value, Formatting.Indented);
					filename = folderName + buyList.Key.ToString("yyyy-MM-dd") + ".json";
					File.WriteAllText(filename, jsonOutput);
				}
			}
		}

		/// <summary>
		/// Outputs the config options used for this sim run.
		/// </summary>
		void OutputConfigFile()
		{
			string jsonOutput = JsonConvert.SerializeObject(Simulator.Config, Formatting.Indented);
			string filename = _outputFolder + "input.json";
			File.WriteAllText(filename, jsonOutput);
		}

		/// <summary>
		/// Outputs the all the statistics for all strategies and the overall stats for each.
		/// </summary>
		void OutputStrategyStats()
		{
			if (Simulator.Config.UseAbbreviatedOutput == true)
			{
				return;
			}

			ConcurrentBag<StrategyStatistics> allStrategyStatistics = new ConcurrentBag<StrategyStatistics>();

#if DEBUG			
			foreach (KeyValuePair<int, ConcurrentBag<Order>> strategy in Simulator.Orders.StrategyDictionary)
#else
			Parallel.ForEach(Simulator.Orders.StrategyDictionary, strategy =>
#endif
			{
				string jsonOutput;
				string folderName;
				string filename;

				ConcurrentBag<Order> orders = strategy.Value;
				if (orders.Count > Simulator.Config.MinRequiredOrders)
				{
					// Get the strategy name but skip the main strategy as it gets process differently.
					string strategyName = orders.First().StrategyName;
					if (strategyName == "MainStrategy")
					{
#if DEBUG
						continue;
#else
						return;
#endif
					}

					Dictionary<int, StrategyTickerPairStatistics> tickersForThisStrategy = new Dictionary<int, StrategyTickerPairStatistics>();
					StrategyStatistics stratStats = new StrategyStatistics(strategyName, orders.First().Type);

					// Catagorize and total all the orders for this strategy by the ticker they are associated with.
					foreach (Order order in orders)
					{
						int tickerHash = order.Ticker.TickerAndExchange.GetHashCode();
						string tickerName = order.Ticker.TickerAndExchange.ToString();

						// If we haven't created the output for this ticker then create it.
						if (!tickersForThisStrategy.ContainsKey(tickerHash))
						{
							tickersForThisStrategy[tickerHash] = new StrategyTickerPairStatistics(strategyName, tickerName, order.DependentIndicatorNames);
						}

						tickersForThisStrategy[tickerHash].AddOrder(order);
						stratStats.AddOrder(order);
					}

					stratStats.CalculateStatistics();
					allStrategyStatistics.Add(stratStats);

					// Output the info about each ticker for this strategy.
					List<StrategyStatistics> overallList = new List<StrategyStatistics>();
					foreach (KeyValuePair<int, StrategyTickerPairStatistics> item in tickersForThisStrategy)
					{
						// This hasn't been calculated yet.
						item.Value.CalculateStatistics();

						// Save the info for this strategy by the ticker.
						jsonOutput = JsonConvert.SerializeObject(item.Value);
						folderName = _outputFolder + "strategies\\" + strategyName;
						filename = folderName + "\\" + item.Value.TickerName + ".json";
						Directory.CreateDirectory(folderName);
						File.WriteAllText(filename, jsonOutput);

						// Save for the overall stats to be outputted later.
						// Order type doesn't matter for tickers and will get ignored in the web display.
						StrategyStatistics tickerStats = new StrategyStatistics(item.Value.TickerName, Order.OrderType.Long);
						tickerStats.InitFromStrategyTickerPairStatistics(item.Value);
						overallList.Add(tickerStats);
					}

					// Output the overall stats for this strategy.
					jsonOutput = JsonConvert.SerializeObject(overallList);
					folderName = _outputFolder + "strategies\\" + strategyName;
					filename = folderName + "\\overall.json";
					Directory.CreateDirectory(folderName);
					File.WriteAllText(filename, jsonOutput);

				}
#if DEBUG
			}
#else
			});
#endif

			string overallJsonOutput = JsonConvert.SerializeObject(allStrategyStatistics.ToArray());
			string overallFilename = _outputFolder + "overall-strategies.json";
			File.WriteAllText(overallFilename, overallJsonOutput);
		}

		/// <summary>
		/// Outputs the price data for each ticker.
		/// </summary>
		void OutputPriceData()
		{
			string filename;
			string jsonOutput;
			string folderName = _outputFolder + "pricedata";

			Directory.CreateDirectory(folderName);
			foreach (KeyValuePair<int, TickerData> item in _tickerData)
			{
				if (IsValidFilename(item.Value.TickerAndExchange.ToString()))
				{
					item.Value.PrepareForSerialization();
					jsonOutput = JsonConvert.SerializeObject(item.Value);
					filename = folderName + "\\" + item.Value.TickerAndExchange.ToString() + ".json";
					File.WriteAllText(filename, jsonOutput);
					item.Value.FreeResourcesAfterSerialization();
				}
			}
		}

		/// <summary>
		/// Outputs all the indicator values for each indicator.
		/// </summary>
		void OutputIndicatorData()
		{
			if (Simulator.Config.UseAbbreviatedOutput == true)
			{
				return;
			}

			string filename;
			string jsonOutput;
			string folderName = _outputFolder + "indicators";

			Directory.CreateDirectory(folderName);
			foreach (KeyValuePair<int, Indicator> item in _indicators)
			{
				item.Value.Serialize(item.Value.Data.NumBars - 1);
				jsonOutput = JsonConvert.SerializeObject(item.Value);
				folderName = _outputFolder + "indicators\\" + item.Value.ToString();
				filename = folderName + "\\" + item.Value.Data.TickerAndExchange.ToString() + ".json";
				Directory.CreateDirectory(folderName);
				File.WriteAllText(filename, jsonOutput);
				item.Value.FreeResourcesAfterSerialization();
			}
		}

		/// <summary>
		/// Outputs the data for the indicators for an order.
		/// </summary>
		public void OutputIndicatorSnapshots(Order order, List<Indicator> indicators, int endBar)
		{
			if (Simulator.Config.UseAbbreviatedOutput == true)
			{
				return;
			}

			string filename;
			string jsonOutput;
			string folderName = _outputFolder + "snapshots\\" + order.OrderId;
			Directory.CreateDirectory(folderName);

			foreach (Indicator ind in indicators)
			{
				ind.Serialize(endBar);
				jsonOutput = JsonConvert.SerializeObject(ind);
				filename = folderName + "\\" + ind.ToString() + ".json";
				File.WriteAllText(filename, jsonOutput);
				ind.FreeResourcesAfterSerialization();
			}
		}

		/// <summary>
		/// Outputs all the overall files for the main strategy.
		/// </summary>
		void OutputMainStrategy()
		{
			if (Simulator.Orders.StrategyDictionary.ContainsKey("MainStrategy".GetHashCode()))
			{
				string jsonOutput;
				string filename;

				ConcurrentBag<Order> mainStrategyOrders = Simulator.Orders.StrategyDictionary["MainStrategy".GetHashCode()];
				StrategyStatistics mainStratStats = new StrategyStatistics("MainStrategy", Order.OrderType.Long);
				foreach (Order order in mainStrategyOrders)
				{
					mainStratStats.AddOrder(order);
				}

				mainStratStats.CalculateStatistics();
				jsonOutput = JsonConvert.SerializeObject(mainStratStats);
				filename = _outputFolder + "overall.json";
				File.WriteAllText(filename, jsonOutput);

				List<Order> mainOrders = mainStrategyOrders.ToList();
				mainOrders.RemoveAll(order => order.Status == Order.OrderStatus.Cancelled);
				jsonOutput = JsonConvert.SerializeObject(mainOrders);
				filename = _outputFolder + "overall-orders.json";
				File.WriteAllText(filename, jsonOutput);

				jsonOutput = JsonConvert.SerializeObject(Simulator.Broker);
				filename = _outputFolder + "overall-account.json";
				File.WriteAllText(filename, jsonOutput);
			}
		}

	} // end class
}
