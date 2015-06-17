using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	class StopSellCondition : SellCondition
	{
		private double _stopPercent;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="stopPercent">Percent in decimal form for the stop target.</param>
		public StopSellCondition(double stopPercent)
			: base()
		{
			_stopPercent = stopPercent;
		}

		/// <summary>
		/// Priority of this sell condition, lower = higher
		/// </summary>
		public override int Priority
		{
			get
			{
				return 10;
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
			double stopPrice = _order.BuyPrice - ((_order.BuyPrice * _stopPercent) * _order.Type);

			if (_order.Type == Order.OrderType.Long)
			{
				// Gapped open below our stop target, so close at the open price.
				if (data.Open[currentBar] <= stopPrice)
				{
					_order.Sell(data.Open[currentBar], currentBar, Order.SellReasonType.StopLoss);
					return true;
				}
				// Either the low or close during this bar was below our stop target,
				// then close at the stop target.
				else if (Math.Min(data.Close[currentBar], data.Low[currentBar]) <= stopPrice)
				{
					_order.Sell(stopPrice, currentBar, Order.SellReasonType.StopLoss);
					return true;
				}
			}
			else if (_order.Type == Order.OrderType.Short)
			{
				// Gapped open above our stop target, so close at the open price.
				if (data.Open[currentBar] >= stopPrice)
				{
					_order.Sell(data.Open[currentBar], currentBar, Order.SellReasonType.StopLoss);
					return true;
				}
				// Either the high or close during this bar was above our stop target,
				// then close at the stop target.
				else if (Math.Max(data.Close[currentBar], data.High[currentBar]) >= stopPrice)
				{
					_order.Sell(stopPrice, currentBar, Order.SellReasonType.StopLoss);
					return true;
				}
			}

			// Didn't sell.
			return false;
		}
	}
}
