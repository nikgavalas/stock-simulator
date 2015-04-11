using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

using StockSimulator.Strategies;

namespace StockSimulator.Core
{
	public class Simulator
	{
		/// <summary>
		/// Holds all the raw price data for the sim.
		/// </summary>
		public TickerDataStore DataStore { get; set; }

		/// <summary>
		/// The list of all the instruments to be simulated to find the best
		/// stocks to buy on a particular day.
		/// </summary>
		public Dictionary<int, BestOfSubStrategies> Instruments { get; set; }

		/// <summary>
		/// Holds all the orders for every strategy and ticker used.
		/// </summary>
		public static OrderHistory Orders { get; set; }

		/// <summary>
		/// Config settings for this simulator run.
		/// </summary>
		public static SimulatorConfig Config { get; set; }

		/// <summary>
		/// Number of bars for this sim run.
		/// </summary>
		public static int NumberOfBars { get; set; }

		/// <summary>
		/// Outputs all the data to json.
		/// </summary>
		public static DataOutputter DataOutput { get; set; }

		/// <summary>
		/// Holds things like account value and cash value.
		/// </summary>
		public static Broker Broker { get; set; }

		/// <summary>
		/// List of all the orders that are not closed.
		/// </summary>
		private List<MainStrategyOrder> _activeOrders { get; set; }

		private CancellationToken _cancelToken;
		private static IProgress<string> _progress = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public Simulator(IProgress<string> progress, CancellationToken cancelToken)
		{
			NumberOfBars = 0;
			DataOutput = new DataOutputter();
			_activeOrders = new List<MainStrategyOrder>();
			_progress = progress;
			_cancelToken = cancelToken;
		}

		/// <summary>
		/// Initializes the sim from the config object.
		/// </summary>
		/// <param name="config">Object with all the parameters that can be used to config how this sim runs</param>
		public bool CreateFromConfig(SimulatorConfig config, TickerDataStore dataStore)
		{
			Config = config;
			DataStore = dataStore;

			if (Config.UseTodaysDate)
			{
				Config.EndDate = DateTime.Now.Date;
			}

			// Load the config file with the instument list for all the symbols that we 
			// want to test.
			Dictionary<int, TickerExchangePair> fileInstruments = new Dictionary<int, TickerExchangePair>();
			string line;
			try
			{
				StreamReader file = new StreamReader(Config.InstrumentListFile);
				while ((line = file.ReadLine()) != null)
				{
					string[] pair = line.Split(',');
					TickerExchangePair newTicker = new TickerExchangePair(pair[1], pair[0]);
					if (fileInstruments.ContainsKey(newTicker.GetHashCode()) == false)
					{
						fileInstruments[newTicker.GetHashCode()] = newTicker;
					}
					else
					{
						WriteMessage("Duplicate ticker in file: " + newTicker.ToString());
					}
				}
			}
			catch (Exception e)
			{
				WriteMessage("Error loading instrument file!\n" + e.Message);
				return false;
			}

			WriteMessage("Initializing ticker data");

			// Add all the symbols as dependent strategies using the bestofsubstrategies
			Instruments = new Dictionary<int, BestOfSubStrategies>();
			foreach (KeyValuePair<int, TickerExchangePair> item in fileInstruments)
			{
				// Get the data for the symbol and save it for later so we can output it.
				TickerData tickerData = DataStore.GetTickerData(item.Value, config.StartDate, config.EndDate);
				if (tickerData != null)
				{
					DataOutput.SaveTickerData(tickerData);
					RunnableFactory factory = new RunnableFactory(tickerData);
					// This strategy will find the best strategy for this instrument everyday and save the value.
					Instruments[item.Value.GetHashCode()] = new BestOfSubStrategies(tickerData, factory);
				}
				else
				{
					WriteMessage("No ticker data for " + item.Value.ToString());
				}
			}

			NumberOfBars = DataStore.SimTickerDates.Count;
			Broker = new Broker(Config.InitialAccountBalance, NumberOfBars);

			return DataStore.SimTickerDates.Count > 0;
		}

		/// <summary>
		/// Preps the instruments for simulation.
		/// </summary>
		public void Initialize()
		{
			WriteMessage("Initializing all the strategies");

			// Reinit all the orders
			Orders = new OrderHistory();

			foreach (KeyValuePair<int, BestOfSubStrategies> task in Instruments)
			{
				task.Value.Initialize();
			}
		}

		/// <summary>
		/// Runs the strategy from start to finish.
		/// </summary>
		public void Run()
		{
			WriteMessage("Running historical analysis for");

			Parallel.ForEach(Instruments, task =>
			{
				WriteMessage("Running " + task.Value.Data.TickerAndExchange.ToString());
				task.Value.Run();
			});

			// Run all to start with so we have the data to simulate with.
			//foreach (KeyValuePair<int, BestOfSubStrategies> task in Instruments)
			//{
			//	WriteMessage("Running " + task.Value.Data.TickerAndExchange.ToString());
			//	task.Value.Run();
			//}

			DateTime startDate = DataStore.SimTickerDates.First().Key;
			DateTime endDate = DataStore.SimTickerDates.Last().Key;

			WriteMessage("Running main strategy for dates " + startDate.ToShortDateString() + " to " + endDate.ToShortDateString());

			// Loop through each date. We use dates because different tickers have different
			// amounts of trading days. And some are trading on certain days while others aren't.
			// This way to just see if the ticker traded that day, and if it did, then we use it.
			// Otherwise we just ignore it for the day it doesn't have.
			int i = 0;
			foreach (KeyValuePair<DateTime, bool> date in DataStore.SimTickerDates)
			{
				OnBarUpdate(date.Key, i++);
			}
		}

		/// <summary>
		/// Outputs all the resuts.
		/// </summary>
		public void Shutdown()
		{
			WriteMessage("Outputting data for web");
			string outputName = DataOutput.OutputData();

			WriteMessage("Idle");

			if (Config.ShouldOpenWebPage == true)
			{
				System.Diagnostics.Process.Start("http://localhost:9000/#/" + outputName + "/");
			}
		}

		/// <summary>
		/// Updates the progres with a message which will be outputted to the log.
		/// </summary>
		/// <param name="message">Mesage to output</param>
		public static void WriteMessage(string message)
		{
			if (_progress != null)
			{
				_progress.Report(message);
			}
		}

		/// <summary>
		/// Called for every bar update. Find instrument (ticker) with the highest
		/// win percent and buy that stock if we have enough money.
		/// </summary>
		/// <param name="currentDate">The date that the sim is on.</param>
		/// <param name="barNumber">The bar number of the sim loop. Not to be used for indexing since each ticker has a different number of bars</param>
		private void OnBarUpdate(DateTime currentDate, int barNumber)
		{
			bool isTradingBar = false;
			List<BestOfSubStrategies> buyList = new List<BestOfSubStrategies>();

			// Add all the tickers that are above our set percent to buy.
			foreach (KeyValuePair<int, BestOfSubStrategies> instrument in Instruments)
			{
				BestOfSubStrategies strat = instrument.Value;
				int currentBar = strat.Data.GetBar(currentDate);
				if (currentBar != -1)
				{
					isTradingBar = true;
					if (strat.Bars[currentBar].HighestPercent >= Config.PercentForBuy)
					{
						buyList.Add(strat);
					}
				}
			}

			if (isTradingBar == true)
			{
				// Sort the list so the instruments that have the highest buy value are first in the list.
				buyList.Sort((x, y) => -1 * x.Bars[x.Data.GetBar(currentDate)].HighestPercent.CompareTo(y.Bars[y.Data.GetBar(currentDate)].HighestPercent));

				// Output the buy list for each day.
				DataOutput.SaveBuyList(buyList, currentDate);

				// Update all the active orders before placing new ones.
				UpdateOrders(currentDate);

				// Buy stocks if we it's a good time.
				if (barNumber >= Config.NumBarsToDelayStart)
				{
					int currentCount = 0;
					for (int i = 0; i < buyList.Count; i++)
					{
						// If the highest percent is enough for a buy, then do it.
						// If not then since the list is sorted, no other ones will
						// be high enough and we can early out of the loop.
						int strategyBarIndex = buyList[i].Data.GetBar(currentDate);
						BestOfSubStrategies.BarStatistics barStats = buyList[i].Bars[strategyBarIndex];
						if (barStats.HighestPercent >= Config.PercentForBuy && barStats.ComboSizeOfHighestStrategy >= Simulator.Config.MinComboSizeToBuy)
						{
							// Don't want to order to late in the strategy where the order can't run it's course.
							// Also, need to have enough money to buy stocks.
							double accountValue = (double)Broker.AccountValue[barNumber > 0 ? barNumber - 1 : barNumber][1];
							double sizeOfOrder = accountValue / Config.MaxBuysPerBar;

							// Make sure we have enough money and also that we have enough time
							// before the end of the sim to complete the order we place.
							if (barNumber < NumberOfBars - Config.MaxBarsOrderOpen && Broker.AccountCash > sizeOfOrder * 1.2)
							{
								bool shouldReallyOrder = true;

								// As a last optional check, see how this ticker has been performing
								// across all strategies. If it's been doing bad then lets not buy it.
								if (Config.ShouldFilterBad)
								{
									StrategyStatistics tickerStats = Orders.GetTickerStatistics(buyList[i].Data.TickerAndExchange, barNumber, Simulator.Config.NumBarsBadFilter);
									if (tickerStats.Gain < 0 || tickerStats.ProfitTargetPercent < Config.BadFilterProfitTarget * 100)
									{
										shouldReallyOrder = false;
									}
								}

								if (shouldReallyOrder == true)
								{
									EnterOrder(buyList[i].Bars[strategyBarIndex].Statistics, buyList[i].Data, strategyBarIndex);
									++currentCount;
								}
							}
						}
						else
						{
							break;
						}

						// We only allow a set number of buys per frame. This is so we don't just buy
						// everything all on one frame so that we can try and get different
						// stocks when we need to.
						if (currentCount >= Config.MaxBuysPerBar)
						{
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Updates all the orders. Adds and subtracts money spent buying and selling.
		/// </summary>
		/// <param name="currentDate">Current date of the simulation</param>
		private void UpdateOrders(DateTime currentDate)
		{
			// Update all the open orders.
			for (int i = 0; i < _activeOrders.Count; i++)
			{
				Order order = _activeOrders[i];

				// Save the previous status befure the update so we can see if it got filled this update.
				Order.OrderStatus previousStatus = order.Status;
				int barNum = order.Ticker.GetBar(currentDate);
				if (barNum != -1)
				{
					order.Update(barNum);
				}

				// If the order has just been filled, then subtract that amount from the account.
				if (previousStatus == Order.OrderStatus.Open && order.Status == Order.OrderStatus.Filled)
				{
					Broker.AccountCash -= order.Value;
				}
				// If the order just finished then add the value back.
				else if (previousStatus == Order.OrderStatus.Filled && order.IsFinished())
				{
					Broker.AccountCash += order.Value;
				}
			}

			// Remove the orders that are finished. This will just remove them from
			// this array but they order will still be saved in the order history.
			_activeOrders.RemoveAll(order => order.IsFinished() || order.Status == Order.OrderStatus.Cancelled);

			double accountValue = 0;
			for (int i = 0; i < _activeOrders.Count; i++)
			{
				Order order = _activeOrders[i];
				accountValue += order.Value;
			}

			// Save the current value at the end of the frame.
			accountValue += Broker.AccountCash;
			Broker.AddValueToList(currentDate, accountValue);
		}

		/// <summary>
		/// Place an order for the main strategy.
		/// </summary>
		private void EnterOrder(List<StrategyStatistics> stats, TickerData ticker, int currentBar)
		{
			MainStrategyOrder order = new MainStrategyOrder(stats, Order.OrderType.Long, ticker, "MainStrategy", currentBar);
			Simulator.Orders.AddOrder(order);
			_activeOrders.Add(order);
		}

	}
}
