using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	public class Simulator
	{
		public SymbolDataStore DataStore
		{
			get;
			set;
		}

		public Simulator()
		{

		}

		public void Initialize()
		{
			MainStrategy mainStrategy = new MainStrategy();
			mainStrategy.Initialize();
		}
	}
}
