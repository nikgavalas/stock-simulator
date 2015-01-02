using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// Container for the best of strategy task to run it for the dates specified. It can then hold
	/// the value of it for each date just like an indicator or strategy.
	/// </summary>
	public class BestOfTask : Strategy
	{
		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"BestOfSubStrategies"
				};

				return deps;
			}
		}

		/// <summary>
		/// Returns the name of this strategy.
		/// </summary>
		/// <returns>The name of this strategy</returns>
		public override string ToString()
		{
			return "BestOfTask";
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ticker">Ticker this task will be simulating</param>
		public BestOfTask(TickerData tickerData, RunnableFactory factory) : base(tickerData, factory)
		{
		}

		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			// For each dependent that IS A STRATEGY
			// See if it's a buy signal today.
			// If yes, add it to the list.

			// Once the list is build, run it through the combo generator to get a list of all the combos.
			// Add orders for each combo.

			// The root runnable class should track the orders for us in the on bar update.


			// This is where the main strategy will be simulated.

			// Update the broker to sell any shares that need a sellin'

			// For each bar
			// Get the value of bestof strategy for this bar. 
			// While we have money left
			// Get the highest symbol that we don't own
			// Buy the appropriate amount of shares based on how much money we want to invest per purchase
		}
	}
}
