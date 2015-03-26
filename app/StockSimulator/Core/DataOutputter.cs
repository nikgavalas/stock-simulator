using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using StockSimulator.Strategies;

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

			[JsonProperty("strategyName")]
			public string StrategyName { get; set; }

			[JsonProperty("percent")]
			public double Percent { get; set; }

			public JsonBuyList(string ticker, string strategyName, double percent)
			{
				Ticker = ticker;
				StrategyName = strategyName;
				Percent = percent;
			}
		}

		private Dictionary<int, TickerData> _tickerData;

		private Dictionary<int, Indicator> _indicators;

		private Dictionary<DateTime, List<JsonBuyList>> _buyLists;


		/// <summary>
		/// Constructor
		/// </summary>
		public DataOutputter()
		{
			_tickerData = new Dictionary<int, TickerData>();
			_indicators = new Dictionary<int, Indicator>();
			_buyLists = new Dictionary<DateTime, List<JsonBuyList>>();
		}

		/// <summary>
		/// Saves the ticker data for outputting later.
		/// </summary>
		/// <param name="data">Ticker data to save</param>
		public void SaveTickerData(TickerData data)
		{
			int key = data.TickerAndExchange.GetHashCode();
			if (!_tickerData.ContainsKey(key))
			{
				_tickerData[key] = data;
			}
		}

		/// <summary>
		/// Saves the indicator data so it can be outtputed at the end.
		/// </summary>
		/// <param name="indicator">The indicator to save</param>
		public void SaveIndicator(Indicator indicator)
		{
			int key = (indicator.Data.TickerAndExchange.ToString() + indicator.ToString()).GetHashCode();
			if (!_indicators.ContainsKey(key))
			{
				_indicators[key] = indicator;
			}
		}

		/// <summary>
		/// Outputs the buy list for the current bar.
		/// </summary>
		/// <param name="buyList">List of stocks that are displaying a reason to buy</param>
		/// <param name="currentDate">The date to use as the output index</param>
		public void SaveBuyList(List<BestOfSubStrategies> buyList, DateTime currentDate)
		{
			// Convert the buy list into better serializable data.
			_buyLists[currentDate] = new List<JsonBuyList>();
			List<JsonBuyList> outputList = _buyLists[currentDate];
			for (int i = 0; i < buyList.Count; i++)
			{
				outputList.Add(new JsonBuyList(buyList[i].Data.TickerAndExchange.ToString(),
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
			string timeString = DateTime.Now.ToString("MM-dd-yyyyTHH-mm-ss-ffff");
			string saveFolderName = GetOutputFolder(timeString);
			Directory.CreateDirectory(saveFolderName.TrimEnd('\\'));

			OutputConfigFile(timeString);
			OutputBuyList(timeString);
			OutputPriceData(timeString);
			OutputIndicatorData(timeString);
			OutputTickerStats(timeString);
			OutputStrategyStats(timeString);
			OutputMainStrategy(timeString);

			return timeString;
		}

		/// <summary>
		/// Uses the date string as a subfolder for the output.
		/// </summary>
		/// <param name="timeString">Date string</param>
		/// <returns>Root folder for the output with the date as the subfolder</returns>
		private string GetOutputFolder(string timeString)
		{
			return Simulator.Config.OutputFolder + "\\" + timeString + "\\";
		}

		/// <summary>
		/// Outputs info about how each ticker has performed.
		/// </summary>
		/// <param name="timeString">Time string used for the folder output</param>
		private void OutputTickerStats(string timeString)
		{
			if (Simulator.Config.UseAbbreviatedOutput == true)
			{
				return;
			}

			List<StrategyStatistics> allTickerStatistics = new List<StrategyStatistics>();

			foreach (KeyValuePair<int, List<Order>> tickerOrder in Simulator.Orders.TickerDictionary)
			{
				List<Order> orders = tickerOrder.Value;
				if (orders.Count > 0)
				{
					string tickerName = orders[0].Ticker.TickerAndExchange.ToString();
					StrategyStatistics tickerStats = new StrategyStatistics(tickerName);

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
					//folderName = GetOutputFolder(timeString) + "ticker-statistics\\" + tickerName;
					//filename = folderName + "\\overall.json";
					//Directory.CreateDirectory(folderName);
					//File.WriteAllText(filename, jsonOutput);


				}
			}

			string jsonOutput = JsonConvert.SerializeObject(allTickerStatistics);
			string filename = GetOutputFolder(timeString) + "overall-tickers.json";
			File.WriteAllText(filename, jsonOutput);
		}

		/// <summary>
		/// Outputs a buy list for each day or just the last day with the option set in the config.
		/// </summary>
		/// <param name="timeString">Time string used for the folder output</param>
		void OutputBuyList(string timeString)
		{
			string jsonOutput;
			string filename;

			// Create the buy list directory to hold all the buy list files.
			string folderName = GetOutputFolder(timeString) + "buylist\\";
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
		/// <param name="timeString">Time string used for the folder output</param>
		void OutputConfigFile(string timeString)
		{
			string jsonOutput = JsonConvert.SerializeObject(Simulator.Config, Formatting.Indented);
			string filename = GetOutputFolder(timeString) + "input.json";
			File.WriteAllText(filename, jsonOutput);
		}

		/// <summary>
		/// Outputs the all the statistics for all strategies and the overall stats for each.
		/// </summary>
		/// <param name="timeString">Time string used for the folder output</param>
		void OutputStrategyStats(string timeString)
		{
			if (Simulator.Config.UseAbbreviatedOutput == true)
			{
				return;
			}

			string jsonOutput;
			string folderName;
			string filename;

			List<StrategyStatistics> allStrategyStatistics = new List<StrategyStatistics>();

			foreach (KeyValuePair<int, List<Order>> strategy in Simulator.Orders.StrategyDictionary)
			{
				List<Order> orders = strategy.Value;
				if (orders.Count > Simulator.Config.MinRequiredOrders)
				{
					// Get the strategy name but skip the main strategy as it gets process differently.
					string strategyName = orders[0].StrategyName;
					if (strategyName == "MainStrategy")
					{
						continue;
					}

					Dictionary<int, StrategyTickerPairStatistics> tickersForThisStrategy = new Dictionary<int, StrategyTickerPairStatistics>();
					StrategyStatistics stratStats = new StrategyStatistics(strategyName);

					// Catagorize and total all the orders for this strategy by the ticker they are associated with.
					for (int i = 0; i < orders.Count; i++)
					{
						int tickerHash = orders[i].Ticker.TickerAndExchange.GetHashCode();
						string tickerName = orders[i].Ticker.TickerAndExchange.ToString();

						// If we haven't created the output for this ticker then creat it.
						if (!tickersForThisStrategy.ContainsKey(tickerHash))
						{
							tickersForThisStrategy[tickerHash] = new StrategyTickerPairStatistics(strategyName, tickerName, orders[i].DependentIndicatorNames);
						}

						tickersForThisStrategy[tickerHash].AddOrder(orders[i]);
						stratStats.AddOrder(orders[i]);
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
						folderName = GetOutputFolder(timeString) + "strategies\\" + strategyName;
						filename = folderName + "\\" + item.Value.TickerName + ".json";
						Directory.CreateDirectory(folderName);
						File.WriteAllText(filename, jsonOutput);

						// Save for the overall stats to be outputted later.
						StrategyStatistics tickerStats = new StrategyStatistics(item.Value.TickerName);
						tickerStats.WinPercent = item.Value.WinPercent;
						tickerStats.LossPercent = item.Value.LossPercent;
						tickerStats.ProfitTargetPercent = item.Value.ProfitTargetPercent;
						tickerStats.StopLossPercent = item.Value.StopLossPercent;
						tickerStats.LengthExceededPercent = item.Value.LengthExceededPercent;
						tickerStats.Gain = item.Value.Gain;
						tickerStats.NumberOfOrders = item.Value.NumberOfOrders;
						overallList.Add(tickerStats);
					}

					// Output the overall stats for this strategy.
					jsonOutput = JsonConvert.SerializeObject(overallList);
					folderName = GetOutputFolder(timeString) + "strategies\\" + strategyName;
					filename = folderName + "\\overall.json";
					Directory.CreateDirectory(folderName);
					File.WriteAllText(filename, jsonOutput);

				}
			}

			jsonOutput = JsonConvert.SerializeObject(allStrategyStatistics);
			filename = GetOutputFolder(timeString) + "overall-strategies.json";
			File.WriteAllText(filename, jsonOutput);
		}

		/// <summary>
		/// Outputs the price data for each ticker.
		/// </summary>
		/// <param name="timeString">Time string used for the folder output</param>
		void OutputPriceData(string timeString)
		{
			if (Simulator.Config.UseAbbreviatedOutput == true)
			{
				return;
			}

			string filename;
			string jsonOutput;
			string folderName = GetOutputFolder(timeString) + "pricedata";

			Directory.CreateDirectory(folderName);
			foreach (KeyValuePair<int, TickerData> item in _tickerData)
			{
				item.Value.PrepareForSerialization();
				jsonOutput = JsonConvert.SerializeObject(item.Value);
				filename = folderName + "\\" + item.Value.TickerAndExchange.ToString() + ".json";
				File.WriteAllText(filename, jsonOutput);
				item.Value.FreeResourcesAfterSerialization();
			}
		}

		/// <summary>
		/// Outputs all the indicator values for each indicator.
		/// </summary>
		/// <param name="timeString">Time string used for the folder output</param>
		void OutputIndicatorData(string timeString)
		{
			if (Simulator.Config.UseAbbreviatedOutput == true)
			{
				return;
			}

			string filename;
			string jsonOutput;
			string folderName = GetOutputFolder(timeString) + "indicators";

			Directory.CreateDirectory(folderName);
			foreach (KeyValuePair<int, Indicator> item in _indicators)
			{
				item.Value.PrepareForSerialization();
				jsonOutput = JsonConvert.SerializeObject(item.Value);
				folderName = GetOutputFolder(timeString) + "indicators\\" + item.Value.ToString();
				filename = folderName + "\\" + item.Value.Data.TickerAndExchange.ToString() + ".json";
				Directory.CreateDirectory(folderName);
				File.WriteAllText(filename, jsonOutput);
				item.Value.FreeResourcesAfterSerialization();
			}
		}

		/// <summary>
		/// Outputs all the overall files for the main strategy.
		/// </summary>
		/// <param name="timeString">Time string used for the folder output</param>
		void OutputMainStrategy(string timeString)
		{
			if (Simulator.Orders.StrategyDictionary.ContainsKey("MainStrategy".GetHashCode()))
			{
				string jsonOutput;
				string filename;

				List<Order> mainStrategyOrders = Simulator.Orders.StrategyDictionary["MainStrategy".GetHashCode()];
				StrategyStatistics mainStratStats = new StrategyStatistics("MainStrategy");
				for (int i = 0; i < mainStrategyOrders.Count; i++)
				{
					Order order = mainStrategyOrders[i];
					mainStratStats.AddOrder(order);
				}

				mainStratStats.CalculateStatistics();
				jsonOutput = JsonConvert.SerializeObject(mainStratStats);
				filename = GetOutputFolder(timeString) + "overall.json";
				File.WriteAllText(filename, jsonOutput);

				jsonOutput = JsonConvert.SerializeObject(mainStrategyOrders);
				filename = GetOutputFolder(timeString) + "overall-orders.json";
				File.WriteAllText(filename, jsonOutput);

				jsonOutput = JsonConvert.SerializeObject(Simulator.Broker);
				filename = GetOutputFolder(timeString) + "overall-account.json";
				File.WriteAllText(filename, jsonOutput);
			}
		}

	} // end class
}
