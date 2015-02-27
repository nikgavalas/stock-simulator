using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace StockSimulator.Core
{
	public class Runnable : Configurable
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

		private bool _isFinishedRunning;

		/// <summary>
		/// Constructor for the runnable.
		/// </summary>
		public Runnable(TickerData tickerData, RunnableFactory factory) : base()
		{
			_isFinishedRunning = false;
			Data = tickerData;

			// Create all the depenent runnables and save them.
			Dependents = new List<Runnable>(DependentNames.Length);
			for (int i = 0; i < DependentNames.Length; i++)
			{
				Runnable dependent = factory.GetRunnable(DependentNames[i]);
				Dependents.Add(dependent);
			}
		}

		/// <summary>
		/// Initializes the runnable.
		/// </summary>
		public virtual void Initialize()
		{
			_isFinishedRunning = false;
			foreach (Runnable runnable in Dependents)
			{
				runnable.Initialize();
			}
		}

		/// <summary>
		/// Runs this object for all bars included in the simulation.
		/// </summary>
		public virtual void Run()
		{
			// Don't need to run it multiple times in a simulation.
			if (_isFinishedRunning == true)
			{
				return;
			}

			// Run all the dependents before this one.
			foreach (Runnable runnable in Dependents)
			{
				runnable.Run();
			}

			Debug.WriteLine("Running: " + ToString());

			// Then run this one. Just use the closing data as the number of bars to run.
			// The dependents will have all the other ticker data but they are all the same
			// size lists.
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
			foreach (Runnable runnable in Dependents)
			{
				runnable.Shutdown();
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
		}

		// TODO: Check for circular depedents and throw an exception.
	}
}
