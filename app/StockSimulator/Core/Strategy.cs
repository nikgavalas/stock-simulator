using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	public class Strategy : Runnable
	{
		public List<bool> WasFound;

		public Strategy(TickerData tickerData, RunnableFactory factory) : base(tickerData, factory)
		{
			WasFound = new List<bool>(tickerData.NumBars);
		}
	}
}
