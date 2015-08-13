using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

using StockSimulator.Strategies;
using System.Collections.Concurrent;
using StockSimulator.Core.BuySellConditions;

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
		public SortedDictionary<string, BestOfRootStrategies> Instruments { get; set; }

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

		private int _startingAccountMonth = 0;
		private double _startingMonthAccountValue = 0.0;
		private bool _isMonthlyLossExceeded = false;

		/// <summary>
		/// Constructor
		/// </summary>
		public Simulator(IProgress<string> progress, CancellationToken cancelToken)
		{
			NumberOfBars = 0;
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
			DataOutput = new DataOutputter();

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
			ConcurrentDictionary<string, BestOfRootStrategies> downloadedInstruments = new ConcurrentDictionary<string, BestOfRootStrategies>();
#if DEBUG			
			foreach (KeyValuePair<string, TickerExchangePair> item in fileInstruments)
#else
			Parallel.ForEach(fileInstruments, item =>
#endif
			{
				// Get the data for the symbol and save it for later so we can output it.
				TickerData tickerData = DataStore.GetTickerData(item.Value, config.StartDate, config.EndDate);
				if (tickerData != null)
				{
					DataOutput.SaveTickerData(tickerData);
					//RunnableFactory factory = new RunnableFactory(tickerData);

					// This strategy will find the best strategy for this instrument everyday and save the value.
					downloadedInstruments[item.Value.ToString()] = new BestOfRootStrategies(tickerData);
				}
				else
				{
					WriteMessage("No ticker data for " + item.Value.ToString());
				}
#if DEBUG
			}
#else
			});
#endif

			// Want to store the instrument data in a sorted way so that we always run things
			// in the same order.
			Instruments = new SortedDictionary<string, BestOfRootStrategies>(downloadedInstruments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

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

			foreach (KeyValuePair<string, BestOfRootStrategies> task in Instruments)
			{
				task.Value.Initialize();
			}
		}

		/// <summary>
		/// Runs the strategy from start to finish.
		/// </summary>
		public void Run()
		{
//			WriteMessage("Running historical analysis for all tickers");

//			int totalInstruments = Instruments.Count;
//			int amountFinished = 0;

//			// Run all to start with so we have the data to simulate with.
//#if DEBUG
//			foreach (KeyValuePair<string, BestOfRootStrategies> task in Instruments)
//#else
//			Parallel.ForEach(Instruments, task =>
//#endif
//			{
//				task.Value.Run();
//				Orders.PurgeTickerOrders(task.Value.Data.TickerAndExchange);

//				Interlocked.Increment(ref amountFinished);
//				WriteMessage((((double)amountFinished / totalInstruments) * 100.0).ToString("##.##") + "% Complete");
//#if DEBUG
//			}
//#else
//			});
//#endif

			DateTime startDate = DataStore.SimTickerDates.First().Key;
			DateTime endDate = DataStore.SimTickerDates.Last().Key;

			WriteMessage("Running main strategy for dates " + startDate.ToShortDateString() + " to " + endDate.ToShortDateString());

			_startingAccountMonth = 0;
			_startingMonthAccountValue = Broker.AccountCash;

			// Loop through each date. We use dates because different tickers have different
			// amounts of trading days. And some are trading on certain days while others aren't.
			// This way to just see if the ticker traded that day, and if it did, then we use it.
			// Otherwise we just ignore it for the day it doesn't have.
			int i = 0;
			int lastPercent = 0;
			foreach (KeyValuePair<DateTime, bool> date in DataStore.SimTickerDates)
			{
				OnBarUpdate(date.Key, i++);

				int percent = Convert.ToInt32(((double)i / DataStore.SimTickerDates.Count) * 100.0);
				if (percent > lastPercent)
				{
					lastPercent = percent;
					WriteMessage(percent.ToString() + "% Complete");
				}
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
			ConcurrentBag<BestOfRootStrategies> buyBag = new ConcurrentBag<BestOfRootStrategies>();

			// Add all the tickers that are above our set percent to buy.
#if DEBUG
			foreach (KeyValuePair<string, BestOfRootStrategies> instrument in Instruments)
#else
						Parallel.ForEach(Instruments, instrument =>
#endif
			{
				BestOfRootStrategies strat = instrument.Value;
				int currentBar = strat.Data.GetBar(currentDate);
				if (currentBar != -1)
				{
					isTradingBar = true;

					// Run the strategy for this bar.
					strat.OnBarUpdate(currentBar);

					// All bars have a zero value by default. So something is found when the 
					// percent is higher than that.
					if (strat.Bars[currentBar].HighestPercent > 0.0)
					{
						// All orders set a min price to place an order.
						if (strat.Data.Open[currentBar] >= Config.MinPriceForOrder)
						{
							// If this is a short order, some brokers have min prices they allow for shorts.
							if (strat.Bars[currentBar].StrategyOrderType == Order.OrderType.Long ||
								(strat.Bars[currentBar].StrategyOrderType == Order.OrderType.Short && strat.Data.Open[currentBar] >= Config.MinPriceForShort))
							{
								buyBag.Add(strat);
							}
						}
					}
				}
#if DEBUG
			}
#else
			});
#endif

			if (isTradingBar == true)
			{
				List<BestOfRootStrategies> buyList = buyBag.ToList();

				// Sort the list so the instruments that have the highest buy value are first in the list.
				buyList.Sort(delegate(BestOfRootStrategies x, BestOfRootStrategies y) 
				{
					int xBar = x.Data.GetBar(currentDate);
					int yBar = y.Data.GetBar(currentDate);
					if (x.Bars[xBar].ExtraOrderInfo.ContainsKey("expectedGain"))
					{
						double xGain = (double)x.Bars[xBar].ExtraOrderInfo["expectedGain"];
						double yGain = (double)y.Bars[yBar].ExtraOrderInfo["expectedGain"];
						return yGain.CompareTo(xGain);
					}
					else
					{
						return y.Bars[yBar].HighestGain.CompareTo(x.Bars[xBar].HighestGain);
					}
				});

				// Output the buy list for each day.
				DataOutput.SaveBuyList(buyList, currentDate);

				// Update all the active orders before placing new ones.
				UpdateOrders(currentDate);

				// Keep tabs on the account cash at the start of the month. We want to 
				// limit our losses for the month.
				UpdateMonthlyAccountValue(currentDate);

				// Buy stocks if it's a good time.
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

						// Also limit our losses and make sure we have not lost too much money this month.
						if (_isMonthlyLossExceeded)
						{
							break;
						}

						// If the highest percent is enough for a buy, then do it.
						// If not then since the list is sorted, no other ones will
						// be high enough and we can early out of the loop.
						int strategyBarIndex = buyList[i].Data.GetBar(currentDate);
						OrderSuggestion barStats = buyList[i].Bars[strategyBarIndex];
						if (barStats.HighestPercent > 0) // TODO: move to combo strategy && barStats.ComboSizeOfHighestStrategy >= Simulator.Config.MinComboSizeToBuy)
						{
							// Make sure we have enough money and also that we have enough time
							// before the end of the sim to complete the order we place.
							if (barNumber < NumberOfBars && Broker.AccountCash > barStats.SizeOfOrder * 1.1)
							{
								double sizeOfOrder = GetOrderSize(strategyBarIndex, barStats.SizeOfOrder, barStats.StrategyOrderType, buyList[i].Data);

								currentCount += EnterOrder(barStats.Statistics,
									barStats.StrategyOrderType, 
									buyList[i].Data, 
									strategyBarIndex,
									sizeOfOrder,
									barStats.DependentIndicators,
									barStats.BuyConditions,
									barStats.SellConditions,
									barStats.ExtraOrderInfo);
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
		/// Returns the size the order should be based on performance.
		/// </summary>
		/// <param name="originalSize">Original size of the order returned from the strategy</param>
		/// <param name="ticker">Ticker we are buying</param>
		/// <returns>See summary</returns>
		private double GetOrderSize(int currentBar, double originalSize, double orderType, TickerData ticker)
		{
			StrategyStatistics stats = Orders.GetStrategyStatistics("MainStrategy", orderType, null, currentBar, Simulator.Config.MaxLookBackBars);

			// Use the Kelly Criterion to compute how much to invest
			// %K = W - [(1 - W) / R]
			// W is the winning probability of the past trades.
			// R is the win/loss ratio from the past trades.
			double winPercent = (orderType == Order.OrderType.Long ? stats.LongWinPercent : stats.ShortWinPercent) / 100.0;
			double avgGain = orderType == Order.OrderType.Long ? stats.LongWinAvg : stats.ShortWinAvg;
			double avgLoss = orderType == Order.OrderType.Long ? stats.LongLossAvg : stats.ShortLossAvg;
			double percentK = avgGain == 0.0 || avgLoss == 0.0 ? 1.0 : winPercent - ((1 - winPercent) / (Math.Abs(avgGain) / Math.Abs(avgLoss)));

			// Minimum investment percent since we got this order for a reason, so we want
			// to invest something.
			if (percentK < 0.25)
			{
				percentK = 0.25;
			}

			return originalSize;// *percentK;
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
			Broker.CurrentAccountValue = accountValue;
			Broker.AddValueToList(currentDate, accountValue);
		}

		/// <summary>
		/// Updates the account value for the start of the month if we are starting
		/// a new month of trading. 
		/// </summary>
		/// <param name="currentDate">Current date of the simulation</param>
		private void UpdateMonthlyAccountValue(DateTime currentDate)
		{
			if (_startingAccountMonth != currentDate.Month)
			{
				_startingAccountMonth = currentDate.Month;
				_startingMonthAccountValue = Broker.CurrentAccountValue;
				_isMonthlyLossExceeded = false;
			}
			else if (_startingMonthAccountValue - Broker.CurrentAccountValue > Simulator.Config.MaxMonthlyLoss)
			{
				_isMonthlyLossExceeded = true;
			}
		}

		/// <summary>
		/// Place an order for the main strategy.
		/// </summary>
		/// <param name="stats">Stats for each strategy combo that occured on this bar</param>
		/// <param name="orderType">Type of order to place for the found strategy (long or short)</param>
		/// <param name="ticker">Ticker we're placing the order on</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <param name="sizeOfOrder">Amount of money to place in this order</param>
		/// <param name="dependentIndicators">List of all the dependent indicators</param>
		/// <param name="buyConditions">All the buy conditions that must be met to fill the order</param>
		/// <param name="sellConditions">Any of the sell conditions trigger a sell</param>
		/// <param name="extraOrderInfo">The extra order info from the substrategy</param>
		private int EnterOrder(
			List<StrategyStatistics> stats,
			double orderType, 
			TickerData ticker,
			int currentBar,
			double sizeOfOrder,
			List<Indicator> dependentIndicators,
			List<BuyCondition> buyConditions,
			List<SellCondition> sellConditions,
			Dictionary<string, object> extraOrderInfo)
		{

			// Save only the names of the indicators for the order to track. The order manager
			// will use the actual indicators to save what they look like when this order
			// was placed.
			List<string> indicatorNames = new List<string>();
			for (int i = 0; i < dependentIndicators.Count; i++)
			{
				indicatorNames.Add(dependentIndicators[i].ToString());
			}
			
			MainStrategyOrder order = new MainStrategyOrder(stats, orderType, ticker, "MainStrategy",
				currentBar, sizeOfOrder, indicatorNames, buyConditions, sellConditions, extraOrderInfo);
			
			Simulator.Orders.AddOrder(order, dependentIndicators, currentBar);

			_activeOrders.Add(order);
			return 1;
		}

	}
}
