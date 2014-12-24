using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;

namespace StockSimulator.Strategies
{
	class SmaStrategy : Strategy
	{
		public SmaStrategy() : base()
		{

		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"Sma"
				};

				return deps;
			}
		}

		public override void Run()
		{
			base.Run();
			Debug.WriteLine("SmaStrategy::Run()");
		}
	}
}
