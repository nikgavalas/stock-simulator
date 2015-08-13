using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	public class LimitBuyCondition : BuyCondition
	{
		private int _maxBarsToFill;
		private double _limitPrice;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="limitPrice">Max price the order can fill at</param>
		/// <param name="maxBarsToFill">How many bars we're allowed to be open and not filled before we cancel the order</param>
		public LimitBuyCondition(double limitPrice, int maxBarsToFill)
			: base()
		{
			_maxBarsToFill = maxBarsToFill;
			_limitPrice = limitPrice;
		}

	/// <summary>
		/// Priority of this buy condition, lower = higher
		/// </summary>
		public override int Priority
		{
			get
			{
				return 90;
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

			// The order can be cancelled if it's open too long.
			if (currentBar + _maxBarsToFill > _order.OpenedBar)
			{
				_order.Cancel();
			}

			TickerData data = _order.Ticker;

			if (_order.Type == Order.OrderType.Long)
			{
				if (data.Open[currentBar] <= _limitPrice)
				{
					_order.Buy(_order.Ticker.Open[currentBar], currentBar, "Limit buy open");
					return true;
				}
				else if (data.Low[currentBar] <= _limitPrice)
				{
					_order.Buy(_limitPrice, currentBar, "Limit buy");
					return true;
				}
			}
			else if (_order.Type == Order.OrderType.Short)
			{
				if (data.Open[currentBar] >= _limitPrice)
				{
					_order.Buy(_order.Ticker.Open[currentBar], currentBar, "Limit buy open");
					return true;
				}
				else if (data.High[currentBar] >= _limitPrice)
				{
					_order.Buy(_limitPrice, currentBar, "Limit buy");
					return true;
				}
			}

			return false;
		}
	}
}
