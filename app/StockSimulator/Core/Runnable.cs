using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace StockSimulator.Core
{
	public class Runnable
	{
		/// <summary>
		/// List of dependent runnables that need to be run before this one.
		/// </summary>
		public virtual string[] DependentNames
		{
			get { return new string[0]; }
		}

		public List<Runnable> Dependents { get; set; }
		public TickerData Data { get; set; }

		protected RunnableFactory _factory;

		private bool _isFinishedRunning;
		private int _lastBarUpdated;

		/// <summary>
		/// Constructor for the runnable.
		/// </summary>
		public Runnable(TickerData tickerData, RunnableFactory factory)
		{
			_factory = factory;
			_isFinishedRunning = false;
			_lastBarUpdated = -1;

			Data = tickerData;
		}

		/// <summary>
		/// Initializes the runnable.
		/// </summary>
		public virtual void Initialize()
		{
			// Create all the depenent runnables and save them.
			Dependents = new List<Runnable>(DependentNames.Length);
			for (int i = 0; i < DependentNames.Length; i++)
			{
				Runnable dependent = _factory.GetRunnable(DependentNames[i]);
				Dependents.Add(dependent);
			}

			_isFinishedRunning = false;
			for (int i = 0; i < Dependents.Count; i++)
			{
				Dependents[i].Initialize();
			}
		}

		/// <summary>
		/// Runs this object for all bars included in the simulation and all it's dependents.
		/// </summary>
		public virtual void Run()
		{
			// Don't need to run it multiple times in a simulation.
			if (_isFinishedRunning == true)
			{
				return;
			}

			// Just use the closing data as the number of bars to run.
			// The dependents all have the same ticker so they are all the same size lists.
			for (int i = 0; i < Data.Close.Count; i++)
			{
				if (Data.IsValidBar(i) == true)
				{
					OnBarUpdate(i);
				}
			}

			_isFinishedRunning = true;
		}

		/// <summary>
		/// Called when the simulation is finished.
		/// </summary>
		public virtual void Shutdown()
		{
			for (int i = 0; i < Dependents.Count; i++)
			{
				Dependents[i].Shutdown();
			}
		}

		/// <summary>
		/// Returns the name of the runnable.
		/// </summary>
		/// <returns>The name of the runnable</returns>
		public override string ToString()
		{
			return "PleaseNameMe!";
		}

		/// <summary>
		/// Called everytime there is a new bar of data. This is called by run.
		/// </summary>
		protected virtual void OnBarUpdate(int currentBar)
		{
			// Prevent from updating multiple times on the same bar.
			if (currentBar <= _lastBarUpdated)
			{
				return;
			}

			// Update all the dependents before this one.
			for (int i = 0; i < Dependents.Count; i++)
			{
				Dependents[i].OnBarUpdate(currentBar);
			}

			_lastBarUpdated = currentBar;
		}

		// TODO: Check for circular depedents and throw an exception.
	}
}
