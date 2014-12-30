using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	public class Simulator
	{
		public TickerDataStore DataStore { get; set; }
		public MainStrategy Strategy { get; set; }

		public Simulator()
		{
			DataStore = new TickerDataStore();
		}

		public void Initialize()
		{
			Strategy = new MainStrategy(DataStore);
			Strategy.Initialize();
		}

		public void Run()
		{
			Strategy.Run();
		}
	}
}
