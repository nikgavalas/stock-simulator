using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	class Runnable : Configurable
	{
		/// <summary>
		/// List of dependent runnables that need to be run before this one.
		/// </summary>
		private List<Runnable> _dependents;

		/// <summary>
		/// Constructor for the runnable.
		/// </summary>
		public Runnable() : base()
		{
			_dependents = new List<Runnable>();
		}

		/// <summary>
		/// Adds another runnable to this runnables dependent list. This will ensure
		/// that the dependents are run before this one.
		/// </summary>
		public void Add(Runnable newDependent)
		{
			_dependents.Add(newDependent);
		}

		/// <summary>
		/// Initializes the runnable.
		/// </summary>
		public virtual void Initialize()
		{
		}

		/// <summary>
		/// Called everytime there is a new bar of data.
		/// </summary>
		public virtual void OnBarUpdate()
		{ 
		}

		/// <summary>
		/// Runs this object for all bars included in the simulation.
		/// </summary>
		public virtual void Run()
		{
		}

		/// <summary>
		/// Called when the simulation is finished.
		/// </summary>
		public virtual void ShutDown()
		{
		}

	}
}
