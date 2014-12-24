using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// The main strategy that will buy the best deemed stocks each day as long as we have money
	/// </summary>
	class MainStrategy
	{
		/// <summary>
		/// The list of all the instruments to be simulated to find the best
		/// stocks to buy on a particular day.
		/// </summary>
		private BestOfTask[] _simulatedInstruments;

		/// <summary>
		/// Initializes all the symbols that we are going to consider when buying.
		/// </summary>
		public void Initialize()
		{
			// Load the config file with the instument list for all the symbols that we 
			// want to test.
			// TODO: for now just hard code it.
			string[] instruments =
			{
				"AAPL",
				"AMD"
			};

			// Add all the symbols as dependent strategies using the bestofsubstrategies
			_simulatedInstruments = new BestOfTask[instruments.Length];
			for (int i = 0; i < instruments.Length; i++)
			{
				_simulatedInstruments[i] = new BestOfTask(instruments[i]);
			}

			// Run all to start with so we have the data to simulate with.
			for (int i = 0; i < _simulatedInstruments.Length; i++)
			{
				_simulatedInstruments[i].RunAll();
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
	}
}
