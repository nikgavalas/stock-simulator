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
		public void CreateFromConfig(SimulatorConfig config, TickerDataStore dataStore)
		{
			Config = config;
			DataStore = dataStore;

			// Load the config file with the instument list for all the symbols that we 
			// want to test.
			List<TickerExchangePair> instruments = new List<TickerExchangePair>();
			string line;
			try
			{
				StreamReader file = new StreamReader(Config.InstrumentListFile);
				while ((line = file.ReadLine()) != null)
				{
					string[] pair = line.Split(',');
					instruments.Add(new TickerExchangePair(pair[1], pair[0]));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Error loading instrument file!\n" + e.Message);
			}

			WriteMessage("Initializing ticker data");

			// Add all the symbols as dependent strategies using the bestofsubstrategies
			Instruments = new Dictionary<int, BestOfSubStrategies>();
			for (int i = 0; i < instruments.Count; i++)
			{
				// Get the data for the symbol and save it for later so we can output it.
				TickerData tickerData = DataStore.GetTickerData(instruments[i], config.startDate, config.endDate);
				if (tickerData != null)
				{
					DataOutput.SaveTickerData(tickerData);
					if (NumberOfBars == 0)
					{
						NumberOfBars = tickerData.Dates.Count;
						Broker = new Broker(Config.InitialAccountBalance, NumberOfBars);
					}

					// Make sure everything we're working with has the same number of bars.
					if (tickerData.Dates.Count == NumberOfBars)
					{
						// The factory is responsible for creating each runnable. There should only be 1 per ticker
						// so that we don't recreate the same runnable per ticker. 
						RunnableFactory factory = new RunnableFactory(tickerData);

						// This strategy will find the best strategy for this instrument everyday and save the value.
						Instruments[instruments[i].GetHashCode()] = new BestOfSubStrategies(tickerData, factory);
					}
					else
					{
						Console.WriteLine("Bars not equal for ticker " + tickerData.TickerAndExchange.ToString());
					}
				}
				else
				{
					Console.WriteLine("No ticker data for " + instruments[i].ToString());
				}
			}
		}

		/// <summary>
		/// Preps the instruments for simulation.
		/// </summary>
		public void Initialize()
		{
			WriteMessage("Initializing all the strategies");

			// Reinit all the orders
			Orders = new OrderHistory();

			// Get the buy list ready.
			DataOutput.InitializeBuyList();

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
			WriteMessage("Running historical analysis");

			// Run all to start with so we have the data to simulate with.
			foreach (KeyValuePair<int, BestOfSubStrategies> task in Instruments)
			{
				task.Value.Run();
			}

			WriteMessage("Running main strategy based on historical analysis");

			// Loop each bar and find the best one of each bar.
			for (int i = 0; i < NumberOfBars; i++)
			{
				OnBarUpdate(i);
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

			System.Diagnostics.Process.Start("http://localhost:9000/#/" + outputName + "/");
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
		/// <param name="currentBar"></param>
		private void OnBarUpdate(int currentBar)
		{
			List<BestOfSubStrategies> buyList = new List<BestOfSubStrategies>();

			// Add all the tickers that are at least showing some sort of activity.
			foreach (KeyValuePair<int, BestOfSubStrategies> instrument in Instruments)
			{
				if (instrument.Value.Bars[currentBar].HighestPercent > 0)
				{
					buyList.Add(instrument.Value);
				}
			}

			// Sort the list so the instruments that have the highest buy value are first in the list.
			buyList.Sort((x, y) => -1 * x.Bars[currentBar].HighestPercent.CompareTo(y.Bars[currentBar].HighestPercent));

			// Output the buy list for each day.
			DataOutput.SaveBuyList(buyList, currentBar);

			// Update all the active orders before placing new ones.
			UpdateOrders(currentBar);

			// Buy stocks if we it's a good time.
			int currentCount = 0;
			for (int i = 0; i < buyList.Count; i++)
			{
				// If the highest percent is enough for a buy, then do it.
				// If not then since the list is sorted, no other ones will
				// be high enough and we can early out of the loop.
				BestOfSubStrategies.BarStatistics barStats = buyList[i].Bars[currentBar];
				if (barStats.HighestPercent > Config.PercentForBuy && barStats.ComboSizeOfHighestStrategy >= Simulator.Config.MinComboSizeToBuy)
				{
					// Don't want to order to late in the strategy where the order can't run it's course.
					// Also, need to have enough money to buy stocks.
					if (currentBar < NumberOfBars - Config.MaxBarsOrderOpen &&
						Broker.AccountCash > Config.SizeOfOrder * 2)
					{
						EnterOrder(buyList[i].Bars[currentBar].Statistics, buyList[i].Data, currentBar);
						++currentCount;
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

		/// <summary>
		/// Updates all the orders. Adds and subtracts money spent buying and selling.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		private void UpdateOrders(int currentBar)
		{
			// Update all the open orders.
			for (int i = 0; i < _activeOrders.Count; i++)
			{
				Order order = _activeOrders[i];

				// Save the previous status befure the update so we can see if it got filled this update.
				Order.OrderStatus previousStatus = order.Status;
				order.Update(currentBar);

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
			Broker.AddValueToList(DataStore.TradeableDateTicker.Dates[currentBar], accountValue);
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
