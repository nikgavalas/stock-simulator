using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// Holds all the data for a symbol (stock, whatever)
	/// </summary>
	class SymbolData
	{
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public DateTime[] Dates { get; set; }
		public double[] Open { get; set; }
		public double[] Close { get; set; }
		public double[] High { get; set; }
		public double[] Low { get; set; }
		public int[] Volume { get; set; }
	}
}
