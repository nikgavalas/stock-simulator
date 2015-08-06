using StockSimulator.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	class StopOneBarTrailingChannel : SellCondition
	{
		private KeltnerChannel _channel;
		private bool _hasHitChannelMax;
		private double _guardStop;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="channel">Indicator to use for the max of the channel</param>
		public StopOneBarTrailingChannel(KeltnerChannel channel)
			: base()
		{
			_channel = channel;
			_hasHitChannelMax = false;
			_guardStop = 0.0;
		}

		/// <summary>
		/// Priority of this sell condition, lower = higher
		/// </summary>
		public override int Priority
		{
			get
			{
				return 20;
			}
		}

		/// <summary>
		/// Called when the order has been filled during the bar update.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation for the order</param>
		/// <returns>True if the order was closed</returns>
		public override bool OnUpdate(int currentBar)
		{
			if (_order.BuyBar == currentBar)
			{
				return false;
			}

			TickerData data = _order.Ticker;

			if (_order.Type == Order.OrderType.Long)
			{
				// Check if we've hit the top of the channel.
				if (data.High[currentBar] >= _channel.Upper[currentBar])
				{
					_hasHitChannelMax = true;
				}

				// If we've hit the channel then guard our profits with a stop based yesterdays prices.
				if (_hasHitChannelMax)
				{
					_guardStop = data.Low[currentBar - 1];
				}

				if (_guardStop > 0.0)
				{
					// Gapped open below our stop target, so close at the open price.
					if (data.Open[currentBar] <= _guardStop)
					{
						_order.Sell(data.Open[currentBar], currentBar, "Channel Pullback");
						return true;
					}
					// Either the low or close during this bar was below our stop target,
					// then close at the stop target.
					else if (Math.Min(data.Close[currentBar], data.Low[currentBar]) <= _guardStop)
					{
						_order.Sell(_guardStop, currentBar, "Channel Pullback");
						return true;
					}
				}
			}
			else if (_order.Type == Order.OrderType.Short)
			{
				// Check if we've hit the bottom of the channel.
				if (data.Low[currentBar] <= _channel.Lower[currentBar])
				{
					_hasHitChannelMax = true;
				}

				// If we've hit the channel then guard our profits with a stop based yesterdays prices.
				if (_hasHitChannelMax)
				{
					_guardStop = data.High[currentBar - 1];
				}

				if (_guardStop > 0.0)
				{
					// Gapped open above our stop target, so close at the open price.
					if (data.Open[currentBar] >= _guardStop)
					{
						_order.Sell(data.Open[currentBar], currentBar, "Channel Pullback");
						return true;
					}
					// Either the high or close during this bar was above our stop target,
					// then close at the stop target.
					else if (Math.Max(data.Close[currentBar], data.High[currentBar]) >= _guardStop)
					{
						_order.Sell(_guardStop, currentBar, "Channel Pullback");
						return true;
					}
				}
			}

			// Didn't sell.
			return false;
		}
	}
}
