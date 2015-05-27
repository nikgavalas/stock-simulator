using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

using StockSimulator.Strategies;
using System.Collections.Concurrent;

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
		public SortedDictionary<string, BestOfSubStrategies> Instruments { get; set; }

		/// <summary>
		/// Holds all the orders for every strategy and ticker used.
		/// </summary>
		public static IOrderHistory Orders { get; set; }

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
			SortedDictionary<string, TickerExchangePair> fileInstruments = new SortedDictionary<string, TickerExchangePair>();
			string line;
			try
			{
				StreamReader file = new StreamReader(Config.InstrumentListFile);
				while ((line = file.ReadLine()) != null)
				{
					string[] pair = line.Split(',');
					TickerExchangePair newTicker = new TickerExchangePair(pair[1], pair[0]);
					string key = newTicker.ToString();
					if (fileInstruments.ContainsKey(key) == false)
					{
						fileInstruments[key] = newTicker;
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
			
			// TODO: there has to be a better place for this!
			//if (Directory.Exists(Simulator.Config.OutputFolder + "\\higher"))
			//{
			//	Directory.Delete(Simulator.Config.OutputFolder + "\\higher", true);
			//}

			// Add all the symbols as dependent strategies using the bestofsubstrategies
			ConcurrentDictionary<string, BestOfSubStrategies> downloadedInstruments = new ConcurrentDictionary<string, BestOfSubStrategies>();
			Parallel.ForEach(fileInstruments, item =>
			//foreach (KeyValuePair<string, TickerExchangePair> item in fileInstruments)
			{
				// Get the data for the symbol and save it for later so we can output it.
				TickerData tickerData = DataStore.GetTickerData(item.Value, config.StartDate, config.EndDate);
				if (tickerData != null)
				{
					DataOutput.SaveTickerData(tickerData);
					RunnableFactory factory = new RunnableFactory(tickerData);

					// This strategy will find the best strategy for this instrument everyday and save the value.
					downloadedInstruments[item.Value.ToString()] = new BestOfSubStrategies(tickerData, factory);
				}
				else
				{
					WriteMessage("No ticker data for " + item.Value.ToString());
				}
			});

			// Want to store the instrument data in a sorted way so that we always run things
			// in the same order.
			Instruments = new SortedDictionary<string, BestOfSubStrategies>(downloadedInstruments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

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
			if (Config.UseAbbreviatedOutput == true)
			{
				Orders = new OrderHistoryAbbreviated();
			}
			else
			{
				Orders = new OrderHistory();
			}

			foreach (KeyValuePair<string, BestOfSubStrategies> task in Instruments)
			{
				task.Value.Initialize();
			}
		}

		/// <summary>
		/// Runs the strategy from start to finish.
		/// </summary>
		public void Run()
		{
			WriteMessage("Running historical analysis for all tickers");

			int totalInstruments = Instruments.Count;
			int amountFinished = 0;

			// Run all to start with so we have the data to simulate with.
			Parallel.ForEach(Instruments, task =>
			//foreach (KeyValuePair<string, BestOfSubStrategies> task in Instruments)
			{
				task.Value.Run();
				Orders.PurgeTickerOrders(task.Value.Data.TickerAndExchange);

				Interlocked.Increment(ref amountFinished);
				WriteMessage((((double)amountFinished / totalInstruments) * 100.0).ToString("##.##") + "% Complete");
			});

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
			foreach (KeyValuePair<string, BestOfSubStrategies> instrument in Instruments)
			{
				BestOfSubStrategies strat = instrument.Value;
				int currentBar = strat.Data.GetBar(currentDate);
				if (currentBar != -1)
				{
					isTradingBar = true;

					// Check to make sure it meets our percentage and price requirements.
					if (strat.Bars[currentBar].HighestPercent >= Config.PercentForBuy &&
						strat.Data.Open[currentBar] >= Config.MinPriceForOrder)
					{
						// If this is a short order, some brokers have min prices they allow for shorts.
						if (strat.Bars[currentBar].StrategyOrderType == Order.OrderType.Long ||
							(strat.Bars[currentBar].StrategyOrderType == Order.OrderType.Short && strat.Data.Open[currentBar] >= Config.MinPriceForShort))
						{
							buyList.Add(strat);
						}
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
						// Only allowed to have a maximum number of orders open at 1 time. This will limit us
						// to working within a budget later in the sim when we make money. Ex. If we start with
						// $100,000 and we double it in 2 years. We don't want to be investing our $200,000 worth
						// of cash. We still want to work with our original amount. This way we can see how much 
						// of a bankroll we'll need to make a living off investing.
						if (_activeOrders.Count >= Config.MaxOpenOrders || currentCount >= Config.MaxOrdersPerBar)
						{
							break;
						}

						// If the highest percent is enough for a buy, then do it.
						// If not then since the list is sorted, no other ones will
						// be high enough and we can early out of the loop.
						int strategyBarIndex = buyList[i].Data.GetBar(currentDate);
						BestOfSubStrategies.BarStatistics barStats = buyList[i].Bars[strategyBarIndex];
						if (barStats.HighestPercent >= Config.PercentForBuy && barStats.ComboSizeOfHighestStrategy >= Simulator.Config.MinComboSizeToBuy)
						{
							// Don't want to order to late in the strategy where the order can't run it's course.
							int lastBarToPlaceOrders = NumberOfBars - (Config.MaxBarsOrderOpenMain + 1);

							// Make sure we have enough money and also that we have enough time
							// before the end of the sim to complete the order we place.
							if (barNumber < lastBarToPlaceOrders && Broker.AccountCash > Config.SizeOfOrder * 1.1)
							{
								bool shouldReallyOrder = true;

								// As a last optional check, see how this ticker has been performing
								// across all strategies. If it's been doing bad then lets not buy it.
								if (Config.ShouldFilterBad)
								{
									StrategyStatistics tickerStats = Orders.GetTickerStatistics(buyList[i].Data.TickerAndExchange, barNumber, Simulator.Config.NumBarsBadFilter);
									if (tickerStats.Gain < 0 || tickerStats.WinPercent < Config.BadFilterProfitTarget * 100)
									{
										shouldReallyOrder = false;
									}
								}

								if (shouldReallyOrder == true)
								{
									currentCount += EnterOrder(buyList[i].Bars[strategyBarIndex].Statistics, barStats.StrategyOrderType, buyList[i].Data, strategyBarIndex);
								}
							}
						}
						else
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

				// If the order was open before the update, then there are two possible outcomes:
				// 1. It filled and did NOT close this update.
				// 2. It filled and did close this update.
				if (previousStatus == Order.OrderStatus.Open)
				{
					// If it filled but didn't close, then subtract it from our cash.
					if (order.Status == Order.OrderStatus.Filled)
					{
						Broker.AccountCash -= order.NumberOfShares * order.BuyPrice;
					}
					// If it closed the same bar then it opened, we haven't had a simulated bar to subtract
					// the cost from our cash, so just add the gain back to the cash.
					else if (order.IsFinished())
					{
						Broker.AccountCash += order.Gain;
					}
				}
				// If the order just finished then add the value back because we deducted the cash from
				// when the order was filled on a previous update.
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
		/// <param name="stats">Stats for each strategy combo that occured on this bar</param>
		/// <param name="orderType">Type of order to place for the found strategy (long or short)</param>
		/// <param name="ticker">Ticker we're placing the order on</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		private int EnterOrder(List<StrategyStatistics> stats, Order.OrderType orderType, TickerData ticker, int currentBar)
		{
			// Check here that the strategy order type matches
			// with the higher timeframe trend. Continue if it doesn't.
			if (Config.UseHigherTimeframeMainStrategy == true && orderType != ticker.HigherTimeframeMomentum[currentBar])
			{
				return 0;
			}

			MainStrategyOrder order = new MainStrategyOrder(stats, orderType, ticker, "MainStrategy", currentBar);
			Simulator.Orders.AddOrder(order, currentBar);
			_activeOrders.Add(order);
			return 1;
		}

	}
}
