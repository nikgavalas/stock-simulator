using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	class ProfitSellCondition : SellCondition
	{
		private double _profitPercent;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="stopPercent">Percent in decimal form for the profit target.</param>
		public ProfitSellCondition(double profitPercent)
			: base()
		{
			_profitPercent = profitPercent;
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
			double profitPrice = _order.BuyPrice + ((_order.BuyPrice * _profitPercent) * _order.Type);

			if (_order.Type == Order.OrderType.Long)
			{
				// Gapped open above our profit target, so close at the open price.
				if (data.Open[currentBar] >= profitPrice)
				{
					_order.Sell(data.Open[currentBar], currentBar, Order.SellReasonType.ProfitTarget);
					return true;
				}
				// Either the high or close during this bar was above our profit target,
				// then close at the profit target.
				else if (Math.Max(data.Close[currentBar], data.High[currentBar]) >= profitPrice)
				{
					_order.Sell(profitPrice, currentBar, Order.SellReasonType.ProfitTarget);
					return true;
				}
			}
			else if (_order.Type == Order.OrderType.Short)
			{
				// Gapped open below our profit target, so close at the open price.
				if (data.Open[currentBar] <= profitPrice)
				{
					_order.Sell(data.Open[currentBar], currentBar, Order.SellReasonType.ProfitTarget);
					return true;
				}
				// Either the low or close during this bar was below our profit target,
				// then close at the profit target.
				else if (Math.Min(data.Close[currentBar], data.Low[currentBar]) <= profitPrice)
				{
					_order.Sell(profitPrice, currentBar, Order.SellReasonType.ProfitTarget);
					return true;
				}
			}

			// Didn't sell.
			return false;
		}
	}
}
