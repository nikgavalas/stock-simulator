using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;

namespace StockSimulator.Strategies
{
	class RsiStrategy : Runnable
	{
		public RsiStrategy() : base()
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
					"Rsi"
				};

				return deps;
			}
		}

		public override void Run()
		{
			base.Run();
			Debug.WriteLine("RsiStrategy::Run()");
		}
	}
}
