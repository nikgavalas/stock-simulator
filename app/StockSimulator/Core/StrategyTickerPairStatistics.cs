using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using StockSimulator.Core.JsonConverters;

namespace StockSimulator.Core
{
	/// <summary>
	/// Tracks the stats of a strategy for a single ticker.
	/// Very similar to StrategyStatistics except it serializes the orders. 
	/// TODO: There is probably a way to combine the two.
	/// </summary>
	class StrategyTickerPairStatistics
	{
		[JsonProperty("winPercent")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double WinPercent { get; set; }

		[JsonProperty("lossPercent")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double LossPercent { get; set; }

		[JsonProperty("gain")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double Gain { get; set; }

		[JsonProperty("name")]
		public string StrategyName { get; set; }

		[JsonProperty("indicators")]
		public List<string> Indicators { get; set; }

		[JsonProperty("orders")]
		public List<Order> Orders { get; set; }

		
		/////////////////// Ignored /////////////////////
		
		[JsonIgnore]
		public string TickerName { get; set; }

		//[JsonIgnore]
		private int _numberOfOrders;

		//[JsonIgnore]
		private int _numberOfWins;

		//[JsonIgnore]
		private int _numberOfLosses;

		//[JsonIgnore]
		private double _totalGain;

		/// <summary>
		/// Just initialize the class.
		/// </summary>
		/// <param name="strategyName">The strategy these statistics are for</param>
		/// <param name="tickerName">Name of the ticker this strategy is working with</param>
		public StrategyTickerPairStatistics(string strategyName, string tickerName, List<string> dependentIndicatorNames)
		{
			TickerName = tickerName;
			StrategyName = strategyName;
			Orders = new List<Order>();
			Indicators = dependentIndicatorNames; 
			_numberOfWins = 0;
			_numberOfLosses = 0;
			_numberOfOrders = 0;
			_totalGain = 0;
		}

		/// <summary>
		/// Add another order to be used in the total.
		/// </summary>
		/// <param name="order">Order to add. Only finished orders are added.</param>
		public void AddOrder(Order order)
		{
			if (order.IsFinished())
			{
				if (order.Gain >= 0)
				{
					++_numberOfWins;
				}
				else
				{
					++_numberOfLosses;
				}

				++_numberOfOrders;
				_totalGain += order.Gain;
				Orders.Add(order);
			}
		}

		/// <summary>
		/// Calculate and save all the statistics.
		/// </summary>
		public void CalculateStatistics()
		{
			Gain = _totalGain;
			WinPercent = 0;
			LossPercent = 0;
			if (_numberOfOrders > 0)
			{
				WinPercent = Math.Round(((double)_numberOfWins / _numberOfOrders) * 100.0);
				LossPercent = Math.Round(((double)_numberOfLosses / _numberOfOrders) * 100.0);
			}
		}
	}
}
