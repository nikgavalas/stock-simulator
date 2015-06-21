using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	public class AboveSetupBarBuyCondition : BuyCondition
	{
		private int _maxBarsToFill;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="strategy">Strategy that when found will trigger a sell</param>
		public AboveSetupBarBuyCondition(int maxBarsToFill)
			: base()
		{
			_maxBarsToFill = maxBarsToFill;

		}

		/// <summary>
		/// Called before the order has been filled during the bar update.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation for the order</param>
		/// <returns>True if the order was bought</returns>
		public override bool OnUpdate(int currentBar)
		{
			base.OnUpdate(currentBar);
			
			TickerData data = _order.Ticker;

			// The order can be cancelled if it's open too long.
			if (currentBar + _maxBarsToFill > _order.OpenedBar)
			{
				_order.Cancel();
			}

			// Long orders are placed 1 tick above the setup bar high.
			if (_order.Type == Order.OrderType.Long)
			{
				double entryPrice = data.High[_order.OpenedBar - 1] + data.TickSize;

				if (data.Open[currentBar] >= entryPrice)
				{
					_order.Buy(data.Open[currentBar], currentBar, "Opened above setup bar high");
					return true;
				}
				else if (data.High[currentBar] >= entryPrice)
				{
					_order.Buy(entryPrice, currentBar, "Crossed setup bar high");
					return true;
				}
			}
			// Short orders are placed 1 tick below the setup bar low.
			else
			{
				double entryPrice = data.Low[_order.OpenedBar - 1] - data.TickSize;

				if (data.Open[currentBar] <= entryPrice)
				{
					_order.Buy(data.Open[currentBar], currentBar, "Opened below setup bar low");
					return true;
				}
				else if (data.Low[currentBar] <= entryPrice)
				{
					_order.Buy(entryPrice, currentBar, "Crossed setup bar low");
					return true;
				}

			}

			return false;
		}
	}
}
