using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	public class DirectionBuyCondition : BuyCondition
	{
		private int _numBarsToWait;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="numBarsToWait">Number of bars to wait before deciding what direction to place the order in</param>
		public DirectionBuyCondition(int numBarsToWait)
			: base()
		{
			_numBarsToWait = numBarsToWait;
		}

		/// <summary>
		/// Called before the order has been filled during the bar update.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation for the order</param>
		/// <returns>True if the order was bought</returns>
		public override bool OnUpdate(int currentBar)
		{
			base.OnUpdate(currentBar);

			if (currentBar - _order.OpenedBar > _numBarsToWait)
			{
				// We'll see if the price has really moved against our initial predicted
				// direction. If it has, we'll changed the direction of the order.
				double openDiff = _order.Ticker.Open[currentBar - 1] - _order.Ticker.Close[_order.OpenedBar];
				double closeDiff = _order.Ticker.Close[currentBar - 1] - _order.Ticker.Close[_order.OpenedBar];

				if (_order.Type == Order.OrderType.Long && openDiff < 0.0 && closeDiff < 0.0)
				{
					_order.Type = Order.OrderType.Short;
					_order.Buy(_order.Ticker.Open[currentBar], currentBar, "Moved short when expected long");
					return true;
				}
				else if (_order.Type == Order.OrderType.Short && openDiff > 0.0 && closeDiff > 0.0)
				{
					_order.Type = Order.OrderType.Long;
					_order.Buy(_order.Ticker.Open[currentBar], currentBar, "Moved long when expected expected");
					return true;
				}
				else
				{
					// We'll just buy the original way we thought it'd go since it moved more or less that way.
					_order.Buy(_order.Ticker.Open[currentBar], currentBar, "Moved long when expected expected");
					return true;
				}
			}

			return false;
		}
	}
}
