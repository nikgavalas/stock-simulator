using StockSimulator.Indicators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	class CrossoverSellCondition : SellCondition
	{
		public enum CrossoverType
		{
			Above,
			Below
		}

		private List<double> _series1;
		private List<double> _series2;
		private CrossoverType _crossoverType;
		private double _value;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="series1">First series to see if it crossed the second</param>
		/// <param name="series2">Second series</param>
		/// <param name="crossoverType">Crosses above or below</param>
		public CrossoverSellCondition(List<double> series1, List<double> series2, CrossoverType crossoverType)
			: base()
		{
			_series1 = series1;
			_series2 = series2;
			_crossoverType = crossoverType;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="series1">First series to see if it crossed the value</param>
		/// <param name="value">Value for the crossover</param>
		/// <param name="crossoverType">Crosses above or below</param>
		public CrossoverSellCondition(List<double> series1, double value, CrossoverType crossoverType)
			: base()
		{
			_series1 = series1;
			_series2 = null;
			_value = value;
			_crossoverType = crossoverType;
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
			// Don't sell if the indicator is wobbily.
			if (_order.BuyBar >= currentBar - 2)
			{
				return false;
			}

			TickerData data = _order.Ticker;
			double crossoverValue = _series2 != null ? _series2[currentBar] : _value;

			if (_crossoverType == CrossoverType.Above && 
				((_series2 == null && DataSeries.CrossAbove(_series1, crossoverValue, currentBar, 0) != -1) ||
				(_series2 != null && DataSeries.CrossAbove(_series1, _series2, currentBar, 0) != -1)))
			{
				_order.Sell(data.Close[currentBar], currentBar, "Cross Above");
				return true;
			}
			else if (_crossoverType == CrossoverType.Below &&
				((_series2 == null && DataSeries.CrossBelow(_series1, crossoverValue, currentBar, 0) != -1) ||
				(_series2 != null && DataSeries.CrossBelow(_series1, _series2, currentBar, 0) != -1)))
			{
				_order.Sell(data.Close[currentBar], currentBar, "Cross Below");
				return true;
			}

			// Didn't sell.
			return false;
		}
	}
}
