using StockSimulator.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	class StopOscillatorZones : SellCondition
	{
		private DtOscillator _oscillator;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="oscillator">Indicator to use for the oscillator zones</param>
		public StopOscillatorZones(DtOscillator oscillator)
			: base()
		{
			_oscillator = oscillator;
		}

		/// <summary>
		/// Priority of this sell condition, lower = higher
		/// </summary>
		public override int Priority
		{
			get
			{
				return 30;
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
				// If the oscillator moves out from the oversold area but then goes back in it
				// then it's not going to be able to go back up so we should sell it.
				if (DataSeries.CrossAbove(_oscillator.SD, DtOscillator.OversoldZone, currentBar, 4) != -1 &&
					DataSeries.CrossBelow(_oscillator.SD, DtOscillator.OversoldZone, currentBar, 0) != -1)
				{
					_order.Sell(data.Close[currentBar], currentBar, "DTosc Fail");
					return true;
				}

				// If we reverse out of the overbought area then the cycle is coming back down and we should
				// take our profits.
				if (DataSeries.CrossBelow(_oscillator.SD, DtOscillator.OverboughtZone, currentBar, 0) != -1)
				{
					_order.Sell(data.Close[currentBar], currentBar, "DTosc Reverse");
					return true;
				}
			}
			// Comments are just the same for short orders but the opposite direction.
			else if (_order.Type == Order.OrderType.Short)
			{
				if (DataSeries.CrossBelow(_oscillator.SD, DtOscillator.OverboughtZone, currentBar, 4) != -1 &&
					DataSeries.CrossAbove(_oscillator.SD, DtOscillator.OverboughtZone, currentBar, 0) != -1)
				{
					_order.Sell(data.Close[currentBar], currentBar, "DTosc Fail");
					return true;
				}

				if (DataSeries.CrossAbove(_oscillator.SD, DtOscillator.OversoldZone, currentBar, 0) != -1)
				{
					_order.Sell(data.Close[currentBar], currentBar, "DTosc Reverse");
					return true;
				}
			}

			// Didn't sell.
			return false;
		}
	}
}
