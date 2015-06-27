using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace StockSimulator.Core
{
	public partial class Runnable
	{
		// Price data for this runnable.
		public TickerData Data { get; set; }

		/// <summary>
		/// List of dependents for this runnable to work.
		/// </summary>
		protected virtual List<Runnable> _dependents;

		/// <summary>
		/// Constructor for the runnable.
		/// </summary>
		public Runnable(TickerData tickerData)
		{
			_dependents = new List<Runnable>();

			Data = tickerData;
		}

		/// <summary>
		/// Initializes the runnable.
		/// </summary>
		public virtual void Initialize()
		{
			for (int i = 0; i < _dependents.Count; i++)
			{
				_dependents[i].Initialize();
			}
		}

		/// <summary>
		/// Called when the simulation is finished.
		/// </summary>
		public virtual void Shutdown()
		{
			for (int i = 0; i < _dependents.Count; i++)
			{
				_dependents[i].Shutdown();
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
		/// Called everytime there is a new bar of data.
		/// <param name="currentBar">Current bar to simulate</param>
		/// </summary>
		public virtual void OnBarUpdate(int currentBar)
		{
		}

	}
}
