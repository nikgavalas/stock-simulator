using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	public class AboveSetupBarBuyCondition : BuyCondition
	{
		/// <summary>
		/// Called before the order has been filled during the bar update.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation for the order</param>
		/// <returns>True if the order was bought</returns>
		public override bool OnUpdate(int currentBar)
		{
			base.OnUpdate(currentBar);

			// Super simple for market orders, just buy.
			_order.Buy(_order.Ticker.Open[currentBar], currentBar, "Market open order");
			return true;
		}
	}
}
