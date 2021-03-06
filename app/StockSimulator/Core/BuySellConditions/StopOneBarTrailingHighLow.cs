﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	class StopOneBarTrailingHighLow : SellCondition
	{
		private bool _buyAndSellSameBar;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="buyAndSellSameBar">If false then we can't sell the same bar we bought in</param>
		public StopOneBarTrailingHighLow(bool buyAndSellSameBar = true)
			: base()
		{
			_buyAndSellSameBar = buyAndSellSameBar;
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
			if (_buyAndSellSameBar == false && _order.BuyBar == currentBar)
			{
				return false;
			}

			TickerData data = _order.Ticker;

			if (_order.Type == Order.OrderType.Long)
			{
				// Setup bar low
				double stopPrice = data.Low[_order.BuyBar - 1] - data.TickSize;

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
				// Setup bar high
				double stopPrice = data.High[_order.BuyBar - 1] + data.TickSize;

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
