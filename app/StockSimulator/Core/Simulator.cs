using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		/// List of all the orders that are not closed.
		/// </summary>
		private List<MainStrategyOrder> _activeOrders { get; set; }

		private double _accountValue;

		/// <summary>
		/// Constructor
		/// </summary>
		public Simulator()
		{
			DataStore = new TickerDataStore();
			NumberOfBars = 0;
			DataOutput = new DataOutputter();
			_activeOrders = new List<MainStrategyOrder>();
		}

		/// <summary>
		/// Initializes the sim from the config object.
		/// </summary>
		/// <param name="config">Object with all the parameters that can be used to config how this sim runs</param>
		public void CreateFromConfig(SimulatorConfig config)
		{
			Config = config;
			_accountValue = Config.InitialAccountBalance;

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

			// Add all the symbols as dependent strategies using the bestofsubstrategies
			Instruments = new Dictionary<int, BestOfSubStrategies>();
			for (int i = 0; i < instruments.Count; i++)
			{
				// Get the data for the symbol and save it for later so we can output it.
				TickerData tickerData = DataStore.GetTickerData(instruments[i], config.startDate, config.endDate);
				DataOutput.SaveTickerData(tickerData);
				if (NumberOfBars == 0)
				{
					NumberOfBars = tickerData.Dates.Count;
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
		}

		/// <summary>
		/// Preps the instruments for simulation.
		/// </summary>
		public void Initialize()
		{
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
			// Run all to start with so we have the data to simulate with.
			foreach (KeyValuePair<int, BestOfSubStrategies> task in Instruments)
			{
				task.Value.Run();
			}

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
			string outputName = DataOutput.OutputData();
			System.Diagnostics.Process.Start("http://localhost:9000/#/" + outputName + "/");
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
			buyList.Sort((x, y) => x.Bars[currentBar].HighestPercent.CompareTo(y.Bars[currentBar].HighestPercent));

			// Output the buy list for each day.
			DataOutput.OutputBuyList(currentBar);

			// Update all the active orders before placing new ones.
			UpdateOrders(currentBar);

			// Buy stocks if we it's a good time.
			int currentCount = 0;
			for (int i = 0; i < buyList.Count; i++)
			{
				// If the highest percent is enough for a buy, then do it.
				// If not then since the list is sorted, no other ones will
				// be high enough and we can early out of the loop.
				if (buyList[i].Bars[currentBar].HighestPercent > Config.PercentForBuy)
				{
					EnterOrder(buyList[i].Bars[currentBar].Statistics, buyList[i].Data, currentBar);
					++currentCount;
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
					_accountValue -= order.Value;
				}
				// If the order just finished then add the value back.
				else if (previousStatus == Order.OrderStatus.Filled && order.IsFinished())
				{
					_accountValue += order.Value;
				}
			}

			// Remove the orders that are finished. This will just remove them from
			// this array but they order will still be saved in the order history.
			_activeOrders.RemoveAll(order => order.IsFinished());
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
