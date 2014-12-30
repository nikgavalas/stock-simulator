using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;

namespace StockSimulator.Indicators
{
	class Rsi : Indicator
	{
		public Rsi() : base()
		{

		}

		public override void Run()
		{
			base.Run();
			Debug.WriteLine("Rsi::Run()");
		}
	}
}
