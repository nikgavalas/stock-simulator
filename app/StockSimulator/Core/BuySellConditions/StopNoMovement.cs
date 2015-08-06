using StockSimulator.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	class StopNoMovement : SellCondition
	{
		private int _numBarsNoMovement;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="numBars">Number of bars with not enough price change.</param>
		public StopNoMovement(int numBars)
			: base()
		{
			_numBarsNoMovement = numBars - 1;
		}

		/// <summary>
		/// Priority of this sell condition, lower = higher
		/// </summary>
		public override int Priority
		{
			get
			{
				return 100;
			}
		}

		/// <summary>
		/// Called when the order has been filled during the bar update.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation for the order</param>
		/// <returns>True if the order was closed</returns>
		public override bool OnUpdate(int currentBar)
		{
			int startBar = currentBar - _numBarsNoMovement;
			if (startBar < 0 || currentBar - _order.BuyBar < _numBarsNoMovement)
			{
				return false;
			}
			
			TickerData data = _order.Ticker;

			// Don't go back too far.
			if (startBar < _order.OpenedBar)
			{
				startBar = _order.OpenedBar;
			}

			// Check the slope of the line of the last n bars is moving in the direction
			// we expect it to.
			double medianAngle = UtilityMethods.LineAngle(startBar, data.Median[startBar], currentBar, data.Median[currentBar]);
			if (medianAngle > -5 && medianAngle < 5)
			{
				_order.Sell(data.Close[currentBar], currentBar, "Consolidated");
				return true;
			}

			// Also if after the last n bars we have a minimal gain (or a loss)
			double percentGain = _order.Type == Order.OrderType.Long ? UtilityMethods.PercentChange(_order.BuyPrice, data.Close[currentBar]) :
				UtilityMethods.PercentChange(data.Close[currentBar], _order.BuyPrice);
			if (percentGain < 0.5)
			{
				_order.Sell(data.Close[currentBar], currentBar, "No Gain");
				return true;
			}

			// Didn't sell.
			return false;
		}
	}
}
