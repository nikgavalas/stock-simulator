using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	class StrategyStatistics
	{
		public double WinPercent { get; set; }
		public double LossPercent { get; set; }
		public double Gain { get; set; }
		public string StrategyName { get; set; }

		public StrategyStatistics(string strategyName, int numOrders, int wins, int losses, double gain)
		{
			StrategyName = strategyName;
			Gain = gain;

			// Calculate the percentages.
			WinPercent = 0;
			LossPercent = 0;
			if (numOrders > 0)
			{
				WinPercent = Math.Round((wins / numOrders) * 100.0);
				LossPercent = Math.Round((losses / numOrders) * 100.0);
			}
		}
	}
}
