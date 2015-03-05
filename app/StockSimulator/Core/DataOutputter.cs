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

		private List<List<JsonBuyList>> _buyLists;


		/// <summary>
		/// Constructor
		/// </summary>
		public DataOutputter()
		{
			_tickerData = new Dictionary<int, TickerData>();
			_indicators = new Dictionary<int, Indicator>();
			_buyLists = new List<List<JsonBuyList>>();
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
		/// Gets the buy list ready to be saved for each bar of the simulation.
		/// </summary>
		public void InitializeBuyList()
		{
			_buyLists = Enumerable.Repeat(new List<JsonBuyList>(), Simulator.NumberOfBars).ToList();
		}

		/// <summary>
		/// Outputs the buy list for the current bar.
		/// </summary>
		/// <param name="buyList">List of stocks that are displaying a reason to buy</param>
		/// <param name="currentBar">The bar to use as the output index</param>
		public void SaveBuyList(List<BestOfSubStrategies> buyList, int currentBar)
		{
			// Convert the buy list into better serializable data.
			_buyLists[currentBar] = new List<JsonBuyList>();
			List<JsonBuyList> outputList = _buyLists[currentBar];
			for (int i = 0; i < buyList.Count; i++)
			{
				outputList.Add(new JsonBuyList(buyList[i].Data.TickerAndExchange.ToString(),
					buyList[i].Bars[currentBar].HighestStrategyName,
					buyList[i].Bars[currentBar].HighestPercent
				));
			}
		}

		/// <summary>
		/// Outputs the data to a folder with the time the simulator finished.
		/// </summary>
		/// <returns>The folder the data was outtputed to</returns>
		public string OutputData()
		{
			string jsonOutput = null;
			string filename = null;
			string folderName = null;

			string timeString = DateTime.Now.ToString("MM-dd-yyyyTHH-mm-ss-ffff");
			string saveFolderName = GetOutputFolder(timeString);
			Directory.CreateDirectory(saveFolderName.TrimEnd('\\'));

			// Create the buy list directory and output all of them for each bar.
			folderName = GetOutputFolder(timeString) + "buylist\\";
			Directory.CreateDirectory(folderName);
			List<DateTime> simulationDates = _indicators.First().Value.Data.Dates;
			for (int i = 0; i < _buyLists.Count; i++)
			{
				jsonOutput = JsonConvert.SerializeObject(_buyLists[i], Formatting.Indented);
				filename = folderName + simulationDates[i].ToString("yyyy-MM-dd") + ".json";
				File.WriteAllText(filename, jsonOutput);
			}

			//////////////////// Info about how each ticker has performed ////////////////

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

			////////////////////// Strategy stats paired with tickers ////////////////////

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
						jsonOutput = JsonConvert.SerializeObject(item.Value, Formatting.Indented);
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
					jsonOutput = JsonConvert.SerializeObject(overallList, Formatting.Indented);
					folderName = GetOutputFolder(timeString) + "strategies\\" + strategyName;
					filename = folderName + "\\overall.json";
					Directory.CreateDirectory(folderName);
					File.WriteAllText(filename, jsonOutput);

				}
			}

			////////////////////// Price data for each ticker ///////////////////////////

			folderName = GetOutputFolder(timeString) + "pricedata";
			Directory.CreateDirectory(folderName);
			foreach (KeyValuePair<int, TickerData> item in _tickerData)
			{
				item.Value.PrepareForSerialization();
				jsonOutput = JsonConvert.SerializeObject(item.Value, Formatting.Indented);
				filename = folderName + "\\" + item.Value.TickerAndExchange.ToString() + ".json";
				File.WriteAllText(filename, jsonOutput);
			}

			////////////////////// Indicator data for each ticker ///////////////////////

			folderName = GetOutputFolder(timeString) + "indicators";
			Directory.CreateDirectory(folderName);
			foreach (KeyValuePair<int, Indicator> item in _indicators)
			{
				item.Value.PrepareForSerialization();
				jsonOutput = JsonConvert.SerializeObject(item.Value, Formatting.Indented);
				folderName = GetOutputFolder(timeString) + "indicators\\" + item.Value.ToString();
				filename = folderName + "\\" + item.Value.Data.TickerAndExchange.ToString() + ".json";
				Directory.CreateDirectory(folderName);
				File.WriteAllText(filename, jsonOutput);
			}

			////////////////////// Main info about all strategies ///////////////////////
			
			jsonOutput = JsonConvert.SerializeObject(allStrategyStatistics, Formatting.Indented);
			filename = GetOutputFolder(timeString) + "overall-strategies.json";
			File.WriteAllText(filename, jsonOutput);

			jsonOutput = JsonConvert.SerializeObject(allTickerStatistics, Formatting.Indented);
			filename = GetOutputFolder(timeString) + "overall-tickers.json";
			File.WriteAllText(filename, jsonOutput);

			///////////////////// Process main strategy /////////////////////////////////
			
			if (Simulator.Orders.StrategyDictionary.ContainsKey("MainStrategy".GetHashCode()))
			{
				List<Order> mainStrategyOrders = Simulator.Orders.StrategyDictionary["MainStrategy".GetHashCode()];
				StrategyStatistics mainStratStats = new StrategyStatistics("MainStrategy");
				for (int i = 0; i < mainStrategyOrders.Count; i++)
				{
					Order order = mainStrategyOrders[i];
					mainStratStats.AddOrder(order);
				}

				mainStratStats.CalculateStatistics();
				jsonOutput = JsonConvert.SerializeObject(mainStratStats, Formatting.Indented);
				filename = GetOutputFolder(timeString) + "overall.json";
				File.WriteAllText(filename, jsonOutput);

				jsonOutput = JsonConvert.SerializeObject(mainStrategyOrders, Formatting.Indented);
				filename = GetOutputFolder(timeString) + "overall-orders.json";
				File.WriteAllText(filename, jsonOutput);

				jsonOutput = JsonConvert.SerializeObject(Simulator.Broker, Formatting.Indented);
				filename = GetOutputFolder(timeString) + "overall-account.json";
				File.WriteAllText(filename, jsonOutput);
			}

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

	}
}
