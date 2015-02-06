using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace StockSimulator.Core
{
	/// <summary>
	/// Handles storing data for output and outputting it to json
	/// </summary>
	class DataOutputter
	{
		private Dictionary<int, TickerData> _tickerData;
		//private string _outputFolder = "C:\\Users\\Nik\\Documents\\Code\\github\\stock-simulator\\output\\output";
		private string _outputFolder = "C:\\Users\\Nik\\Documents\\github\\stock-simulator\\output\\output";

		/// <summary>
		/// Constructor
		/// </summary>
		public DataOutputter()
		{
			_tickerData = new Dictionary<int, TickerData>();
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
		/// Outputs the buy list for the current bar.
		/// </summary>
		/// <param name="currentBar">The bar to use as the output index</param>
		public void OutputBuyList(int currentBar)
		{

		}

		/// <summary>
		/// Outputs the data to a folder with the time the simulator finished.
		/// </summary>
		/// <returns>The folder the data was outtputed to</returns>
		public string OutputData()
		{
			JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
			string jsonOutput = null;
			string filename = null;

			string timeString = DateTime.Now.ToString("MM-dd-yyyyTHH-mm-ss-ffff");
			string saveFolderName = GetOutputFolder(timeString);
			Directory.CreateDirectory(saveFolderName.TrimEnd('\\'));

			List<StrategyStatistics> allStrategyStatistics = new List<StrategyStatistics>();

			foreach (KeyValuePair<int, List<Order>> strategy in Simulator.Orders.StrategyDictionary)
			{
				// TODO: skip main strategy here. Later get main strategy and then for each 
				// order add it to a class that holds all the list items so that it can
				// output all the orders when serialized.
				// Also need a class that holds the win/loss percent ect and should be 
				// calculated when looping through the orders as well. Then that class can be serialized
				// using the normal methods.

				List<Order> orders = strategy.Value;
				if (orders.Count > 0)
				{
					// Get the strategy name but skip the main strategy as it gets process differently.
					string strategyName = orders[0].StrategyName;
					if (strategyName == "MainStrategy")
					{
						continue;
					}

					Dictionary<int, StrategyTicker> tickersForThisStrategy = new Dictionary<int, StrategyTicker>();
					StrategyStatistics stratStats = new StrategyStatistics(strategyName);

					// Catagorize and total all the orders for this strategy by the ticker they are associated with.
					for (int i = 0; i < orders.Count; i++)
					{
						int tickerHash = orders[i].Ticker.TickerAndExchange.GetHashCode();
						string tickerName = orders[i].Ticker.TickerAndExchange.ToString();
					
						// If we haven't created the output for this ticker then creat it.
						if (!tickersForThisStrategy.ContainsKey(tickerHash))
						{
							tickersForThisStrategy[tickerHash] = new StrategyTicker(tickerName);
						}

						tickersForThisStrategy[tickerHash].AddOrder(orders[i]);
						stratStats.AddOrder(orders[i]);
					}

					stratStats.CalculateStatistics();
					allStrategyStatistics.Add(stratStats);

					// Output the info about each ticker for this strategy.
					List<JsonOverallStrategy> overallList = new List<JsonOverallStrategy>();
					foreach (KeyValuePair<int, StrategyTicker> item in tickersForThisStrategy)
					{
						jsonOutput = jsonSerializer.Serialize(item.Value.GetAsJson());
						filename = GetOutputFolder(timeString) + "strategies\\" + strategyName + "\\" + item.Value.TickerName + ".json";
						File.WriteAllText(filename, jsonOutput);

						// Save for the overall stats to be outputted later.
						overallList.Add(item.Value.GetJsonOverall());
					}

					// Output the overall stats for this strategy.
					jsonOutput = jsonSerializer.Serialize(overallList);
					filename = GetOutputFolder(timeString) + "strategies\\" + strategyName + "\\overall.json";
					File.WriteAllText(filename, jsonOutput);

				}
			}

			////////////////////// Main info about all strategies ///////////////////////
			//MemoryStream serializerStream = new MemoryStream();
			//DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(StrategyStatistics));
			//serializer.WriteObject(serializerStream, p);
			jsonOutput = jsonSerializer.Serialize(allStrategyStatistics);
			filename = GetOutputFolder(timeString) + "overall-strategies.json";
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

				jsonOutput = jsonSerializer.Serialize(mainStratStats);
				filename = GetOutputFolder(timeString) + "overall.json";
				File.WriteAllText(filename, jsonOutput);

				jsonOutput = jsonSerializer.Serialize(mainStrategyOrders);
				filename = GetOutputFolder(timeString) + "overall-orders.json";
				File.WriteAllText(filename, jsonOutput);
			}

			return saveFolderName;
		}

		/// <summary>
		/// Uses the date string as a subfolder for the output.
		/// </summary>
		/// <param name="timeString">Date string</param>
		/// <returns>Root folder for the output with the date as the subfolder</returns>
		private string GetOutputFolder(string timeString)
		{
			return _outputFolder + "\\" + timeString + "\\";
		}




		/// <summary>
		/// Simple class for the structure of output for a strategies overall stats by ticker.
		/// </summary>
		private class JsonOverallStrategy
		{
			public string ticker { get; set; }
			public double winPercent { get; set; }
			public double lossPercent { get; set; }
			public double gain { get; set; }
		}

		/// <summary>
		/// Simple class for the output structure of an order in json.
		/// </summary>
		private class JsonOrder
		{
			public string ticker { get; set; }
			public long id { get; set; }
			public double buyPrice { get; set; }
			public double sellPrice { get; set; }
			public string buyDate { get; set; }
			public string sellDate { get; set; }
			public int numShares { get; set; }
			public double gain { get; set; }
		}

		/// <summary>
		/// Simple class for the structure of the strategy output by the ticker.
		/// </summary>
		private class JsonStrategyTicker
		{
			public List<string> indicators { get; set; }
			public Dictionary<string, JsonOrder> orders { get; set; }
		}

		/// <summary>
		/// Deals with calculating the overall stats fo the strategy by ticker. Also holds all the orders for
		/// this strategy by the ticker so that they can all be outputted.
		/// </summary>
		private class StrategyTicker
		{
			private List<Order> _orders;
			private int _numberOfWiners;
			private int _numberOfLosers;
			private double _totalGain;

			public string TickerName { get; set; }

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="tickerName">Ticker and exchange name</param>
			public StrategyTicker(string tickerName)
			{
				TickerName = tickerName;
				_numberOfWiners = 0;
				_numberOfLosers = 0;
				_totalGain = 0;
				_orders = new List<Order>();
			}

			/// <summary>
			/// Add another order to be used in the total.
			/// </summary>
			/// <param name="order">Order to add. Only finished orders are added.</param>
			public void AddOrder(Order order)
			{
				if (order.IsFinished())
				{
					if (order.GetGain() >= 0)
					{
						++_numberOfWiners;
					}
					else
					{
						++_numberOfLosers;
					}

					_totalGain += order.GetGain();
					_orders.Add(order);
				}
			}

			/// <summary>
			/// Returns this object in a json friendly way.
			/// </summary>
			/// <returns>The json object to be serialized=</returns>
			public JsonStrategyTicker GetAsJson()
			{
				JsonStrategyTicker json = new JsonStrategyTicker();
				string strategyName = _orders[0].StrategyName;
				json.indicators.AddRange(strategyName.Split('-'));

				for (int i = 0; i < _orders.Count; i++)
				{
					Order o = _orders[i];
					JsonOrder jsonOrder = new JsonOrder();
					jsonOrder.ticker = o.Ticker.TickerAndExchange.ToString();
					jsonOrder.id = o.OrderId;
					jsonOrder.buyPrice = Math.Round(o.BuyPrice, 2);
					jsonOrder.sellPrice = Math.Round(o.SellPrice, 2);
					jsonOrder.buyDate = o.BuyDate.ToShortDateString();
					jsonOrder.sellDate = o.SellDate.ToShortDateString();
					jsonOrder.numShares = o.NumberOfShares;
					jsonOrder.gain = Math.Round(o.GetGain(), 2);
					json.orders[o.OrderId.ToString()] = jsonOrder;
				}
				return json;
			}

			/// <summary>
			/// Returns an object that will be serialized to json.
			/// </summary>
			public JsonOverallStrategy GetJsonOverall()
			{
				JsonOverallStrategy overall = new JsonOverallStrategy();
				overall.ticker = TickerName;
				overall.gain = _totalGain;

				// Calculate the percentages.
				if (_orders.Count > 0)
				{
					overall.winPercent = Math.Round((_numberOfWiners / _orders.Count) * 100.0);
					overall.lossPercent = Math.Round((_numberOfLosers / _orders.Count) * 100.0);
				}
				else
				{
					overall.winPercent = 0;
					overall.lossPercent = 0;
				}

				return overall;
			}
		}
	}
}
