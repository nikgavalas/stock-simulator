using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	class StrategyFoundSellCondition : SellCondition
	{
		private Strategy _strategy;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="strategy">Strategy that when found will trigger a sell</param>
		public StrategyFoundSellCondition(Strategy strategy)
			: base()
		{
			_strategy = strategy;
		}

		/// <summary>
		/// Priority of this sell condition, lower = higher
		/// </summary>
		public override int Priority
		{
			get
			{
				return 12;
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

			if (_strategy.WasFound[currentBar] == true)
			{
				string sellReason = Order.SellReasonType.StrategyFound + " " + _strategy.ToString();

				// Sell at the open price for the next day since we would only find out 
				// about this event after the market is closed.
				if (currentBar < data.NumBars - 1)
				{
					_order.Sell(data.Open[currentBar + 1], currentBar + 1, sellReason);
				}
				// If this is the end of the data we have then close the order at the
				// end of the market day.
				else
				{
					_order.Sell(data.Close[currentBar], currentBar, sellReason);
				}
				
				return true;
			}

			// Didn't sell.
			return false;
		}
	}
}
