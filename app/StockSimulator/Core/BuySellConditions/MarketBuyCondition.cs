using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	public class MarketBuyCondition : BuyCondition
	{
		/// <summary>
		/// Priority of this buy condition, lower = higher
		/// </summary>
		public override int Priority
		{
			get
			{
				return 100;
			}
		}

		/// <summary>
		/// Called before the order has been filled during the bar update.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation for the order</param>
		/// <returns>True if the order was bought</returns>
		public override bool OnUpdate(int currentBar)
		{
			base.OnUpdate(currentBar);

			//if (_order.OpenedBar == currentBar)
			//{
			//	return false;
			//}

			// Super simple for market orders, just buy.
			_order.Buy(_order.Ticker.Open[currentBar], currentBar, "Market open order");
			return true;
		}
	}
}
