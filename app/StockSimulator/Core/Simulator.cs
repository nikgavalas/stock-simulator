using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public Dictionary<int, BestOfTask> Instruments { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public Simulator()
		{
			DataStore = new TickerDataStore();
		}

		/// <summary>
		/// Initializes the sim from the config object.
		/// </summary>
		/// <param name="config">Object with all the parameters that can be used to config how this sim runs</param>
		public void CreateFromConfig(SimulatorConfig config)
		{
			// Load the config file with the instument list for all the symbols that we 
			// want to test.
			// TODO: for now just hard code it.
			TickerExchangePair[] instruments =
			{
				new TickerExchangePair("NASDAQ", "AAPL")
			};

			// Add all the symbols as dependent strategies using the bestofsubstrategies
			Instruments = new Dictionary<int, BestOfTask>();
			for (int i = 0; i < instruments.Length; i++)
			{
				// Get the data for the symbol.
				TickerData tickerData = DataStore.GetTickerData(instruments[i], config.startDate, config.endDate);
				RunnableFactory factory = new RunnableFactory(tickerData);
				Instruments[instruments[i].GetHashCode()] = new BestOfTask(tickerData, factory);
			}
		}

		/// <summary>
		/// Preps the instruments for simulation.
		/// </summary>
		public void Initialize()
		{
			foreach (KeyValuePair<int, BestOfTask> task in Instruments)
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
			foreach (KeyValuePair<int, BestOfTask> task in Instruments)
			{
				task.Value.Run();
			}

		}

	}
}
