using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;

namespace StockSimulator.Indicators
{
	class Macd : Indicator
	{
		public Macd() : base()
		{

		}


		public override void Run()
		{
			base.Run();
			Debug.WriteLine("Macd::Run()");
		}
	}
}
