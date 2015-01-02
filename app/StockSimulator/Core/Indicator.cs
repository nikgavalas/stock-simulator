using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	public class Indicator : Runnable
	{
		public Indicator(TickerData tickerData) : base(tickerData)
		{

		}
	}
}
