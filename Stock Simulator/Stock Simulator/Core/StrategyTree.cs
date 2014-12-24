using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Strategies;
using StockSimulator.Indicators;
using System.Diagnostics;

namespace StockSimulator.Core
{
	/// <summary>
	/// Holds all the added runnables and their dependencies. Calls them in the order of least dependencies
	/// to more dependencies.
	/// </summary>
	class StrategyTree
	{
		List<Runnable> _runnables;
		DataSeries _instrumentData;

 		/// <summary>
		/// </summary>
		public StrategyTree(string startingStrategyName, DataSeries instrumentData)
		{
			_runnables = new List<Runnable>();
			_instrumentData = instrumentData;

			// Create this strategy from the factory and get all the dependent factories added too
			Runnable startingStrategy = RunnableFactory.CreateRunnable(startingStrategyName, instrumentData);
			AddRunnable(startingStrategy);
		}

		/// <summary>
		/// Runs all the Runnables that were added.
		/// </summary>
		public void RunAll()
		{
			for (int i = 0; i < _runnables.Count; i++)
			{
				_runnables[i].Run();
			}
		}

		/// <summary>
		/// Recursive function that adds a runnable and all its dependents to the list so that 
		/// the dependents can be run before the the runnable that depends on them.
		/// </summary>
		/// <param name="runnable">Runnable to add</param>
		private void AddRunnable(Runnable runnable)
		{
			// Add all the dependents before this one.
			for (int i = 0; i < runnable.DependentNames.Length; i++)
			{
				Runnable dependent = RunnableFactory.CreateRunnable(runnable.DependentNames[i], _instrumentData);
				AddRunnable(dependent);
			}

			// Check to make sure we don't add dependencies in an infinite way.
			if (_runnables.Contains(runnable))
			{
				Debug.WriteLine("Circular dependency when adding runnables");
			}
			else
			{
				_runnables.Add(runnable);
			}
		}


	}
}
