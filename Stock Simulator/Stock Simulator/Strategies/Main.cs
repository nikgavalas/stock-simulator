using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core;

namespace StockSimulator.Strategies
{
	/// <summary>
	/// The main strategy that will buy the best deemed stocks each day as long as we have money
	/// </summary>
	class Main : Strategy
	{
		/// <summary>
		/// Initializes all the symbols that we are going to consider when buying.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			// Load the config file with the instument list for all the symbols that we 
			// want to test.

			// Add all the symbols as dependent strategies using the bestofsubstrategies 


		}

		/// <summary>
		/// Called on each bar update. Will determine which stocks we should buy.
		/// </summary>
		public override void OnBarUpdate()
		{
			base.OnBarUpdate();

			// Update the broker to sell any shares that need a sellin'

			// For each bar
			// Get the value of bestof strategy for this bar. 
			// While we have money left
			// Get the highest symbol that we don't own
			// Buy the appropriate amount of shares based on how much money we want to invest per purchase
		}
	}
}
