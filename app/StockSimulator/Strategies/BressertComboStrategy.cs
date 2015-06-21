using StockSimulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Strategies
{
	class BressertComboStrategy : ComboStrategy
	{
		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		/// <param name="factory">Factory for creating dependents</param>
		public BressertComboStrategy(TickerData tickerData, RunnableFactory factory) 
			: base(tickerData, factory)
		{
			_minComboSize = Simulator.Config.BressertComboMinComboSize;
			_maxComboSize = Simulator.Config.BressertComboMaxComboSize;
			_maxBarsOpen = Simulator.Config.BressertComboMaxBarsOpen;
			_comboLeewayBars = Simulator.Config.BressertComboLeewayBars;
			_minPercentForBuy = Simulator.Config.BressertComboPercentForBuy;
			_sizeOfOrder = Simulator.Config.BressertComboSizeOfOrder;
			_stopPercent = Simulator.Config.BressertComboStopPercent;
			_namePrefix = "BC-";
		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					// Bull strategies
					"BullBressertDss10",
					"BullBressertDss5",
					"BullRsi3m3",

					// Bear strategies
					"BearBressertDss10",
					"BearBressertDss5",
					"BearRsi3m3"
				};

				return deps;
			}
		}

	}
}
