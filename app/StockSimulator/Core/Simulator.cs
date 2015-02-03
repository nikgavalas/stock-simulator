using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		/// Outputs all the data to json
		/// </summary>
		private DataOutputter _dataOutput;

		private double _accountValue;

		/// <summary>
		/// Constructor
		/// </summary>
		public Simulator()
		{
			DataStore = new TickerDataStore();
			_dataOutput = new DataOutputter();
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
			// TODO: for now just hard code it.
			TickerExchangePair[] instruments =
			{
				new TickerExchangePair("NASDAQ", "AAPL")
			};

			// Add all the symbols as dependent strategies using the bestofsubstrategies
			Instruments = new Dictionary<int, BestOfSubStrategies>();
			for (int i = 0; i < instruments.Length; i++)
			{
				// Get the data for the symbol and save it for later so we can output it.
				TickerData tickerData = DataStore.GetTickerData(instruments[i], config.startDate, config.endDate);
				_dataOutput.SaveTickerData(tickerData);

				// The factory is responsible for creating each runnable. There should only be 1 per ticker
				// so that we don't recreate the same runnable per ticker. 
				RunnableFactory factory = new RunnableFactory(tickerData);

				// This strategy will find the best strategy for this instrument everyday and save the value.
				Instruments[instruments[i].GetHashCode()] = new BestOfSubStrategies(tickerData, factory);
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
		}

		/// <summary>
		/// Outputs all the resuts.
		/// </summary>
		public void Shutdown()
		{
			// Call dataoutputter to output all the json.
		}

		/// <summary>
		/// Called for every bar update. Find instrument (ticker) with the highest
		/// win percent and buy that stock if we have enough money.
		/// </summary>
		/// <param name="currentBar"></param>
		private void OnBarUpdate(int currentBar)
		{
			SortedList<double, BestOfSubStrategies> buyList = new SortedList<double, BestOfSubStrategies>();

			// Add all the tickers that are at least showing some sort of activity.
			for (int i = 0; i < Instruments.Count; i++)
			{
				if (Instruments[i].Bars[currentBar].HighestPercent > 0)
				{
					buyList.Add(Instruments[i].Bars[currentBar].HighestPercent, Instruments[i]);
				}
			}

			// Output the buy list for each day.
			_dataOutput.OutputBuyList(currentBar);


		}
	}
}
