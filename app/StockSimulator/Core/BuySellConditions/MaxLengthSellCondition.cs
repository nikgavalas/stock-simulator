using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	class MaxLengthSellCondition : SellCondition
	{
		private int _maxBarsOpen;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="maxBarsOpen">Max number of bars open</param>
		public MaxLengthSellCondition(int maxBarsOpen)
			: base()
		{
			// Subtract 1 because we can buy and sell on the same bar which when
			// doing the math between the current bar and buy bar is 0. So in that
			// case its max open is 0 but for config purposes its easier to see it
			// as max bars as 1.
			_maxBarsOpen = maxBarsOpen - 1;
		}

		/// <summary>
		/// Priority of this sell condition, lower = higher
		/// </summary>
		public override int Priority
		{
			get
			{
				return 11;
			}
		}

		/// <summary>
		/// Called when the order has been filled during the bar update.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation for the order</param>
		/// <returns>True if the order was closed</returns>
		public override bool OnUpdate(int currentBar)
		{
			TickerData data = _order.Ticker;

			if (currentBar - _order.BuyBar >= _maxBarsOpen)
			{
				_order.Sell(data.Close[currentBar], currentBar, Order.SellReasonType.LengthExceeded);
				return true;
			}

			// Didn't sell.
			return false;
		}
	}
}
