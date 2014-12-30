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
				Instruments[instruments[i].GetHashCode()] = new BestOfTask(instruments[i]);
			}
		}

		/// <summary>
		/// Preps the instruments for simulation.
		/// </summary>
		public void Initialize()
		{
			foreach (KeyValuePair<int, BestOfTask> task in Instruments)
			{
				task.Value.Initialize(DataStore);
			}

		}

		/// <summary>
		/// Called on each bar update. Will determine which stocks we should buy.
		/// </summary>
		public void OnBarUpdate()
		{

			// Update the broker to sell any shares that need a sellin'

			// For each bar
			// Get the value of bestof strategy for this bar. 
			// While we have money left
			// Get the highest symbol that we don't own
			// Buy the appropriate amount of shares based on how much money we want to invest per purchase
		}

		/// <summary>
		/// Runs the strategy from start to finish.
		/// </summary>
		public void Run()
		{
			// Run all to start with so we have the data to simulate with.
			foreach (KeyValuePair<int, BestOfTask> task in Instruments)
			{
				task.Value.RunAll();
			}

		}

	}
}
