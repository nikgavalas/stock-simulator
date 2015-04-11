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
	/// Calculates statistics from orders that are added.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class StatisticsCalculator
	{
		[JsonProperty("numberOfOrders")]
		public int NumberOfOrders { get; set; }

		[JsonProperty("winPercent")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double WinPercent { get; set; }

		[JsonProperty("lossPercent")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double LossPercent { get; set; }

		[JsonProperty("profitTargetPercent")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double ProfitTargetPercent { get; set; }

		[JsonProperty("stopLossPercent")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double StopLossPercent { get; set; }

		[JsonProperty("lengthExceededPercent")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double LengthExceededPercent { get; set; }

		[JsonProperty("gain")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double Gain { get; set; }

		[JsonProperty("averageOrderLength")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double AverageOrderLength { get; set; }

		[JsonProperty("averageProfitOrderLength")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double AverageProfitOrderLength { get; set; }

		[JsonProperty("averageStopOrderLength")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double AverageStopOrderLength { get; set; }

		// Not serialized!
		public virtual List<Order> Orders { get; set; }

		private long _numberOfWins = 0;
		private long _numberOfLosses = 0;
		private long _numberOfProfitTargets = 0;
		private long _numberOfStopLosses = 0;
		private long _numberOfLengthExceeded = 0;
		private long _totalLengthOfAllOrders = 0;
		private long _totalLengthOfProfitOrders = 0;
		private long _totalLengthOfStopOrders = 0;

		private double _totalGain = 0;
	
		/// <summary>
		/// Constructor that doesn't calculate the stats to be used with add order.
		/// </summary>
		public StatisticsCalculator()
		{
			Orders = new List<Order>();
		}

		/// <summary>
		/// Adds an order to the list so the percents can be calculated later.
		/// </summary>
		/// <param name="order"></param>
		public void AddOrder(Order order)
		{
			if (order.IsFinished())
			{
				Orders.Add(order);

				++NumberOfOrders;
				if (order.Gain > 0)
				{
					++_numberOfWins;
				}
				else
				{
					++_numberOfLosses;
				}

				if (order.Status == Order.OrderStatus.ProfitTarget)
				{
					++_numberOfProfitTargets;
					_totalLengthOfProfitOrders += order.SellBar - order.BuyBar;
				}
				else if (order.Status == Order.OrderStatus.StopTarget)
				{
					++_numberOfStopLosses;
					_totalLengthOfStopOrders += order.SellBar - order.BuyBar;
				}
				else if (order.Status == Order.OrderStatus.LengthExceeded)
				{
					++_numberOfLengthExceeded;
				}

				_totalLengthOfAllOrders += order.SellBar - order.BuyBar;
				_totalGain += order.Gain;
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
			ProfitTargetPercent = 0;
			StopLossPercent = 0;
			LengthExceededPercent = 0;

			AverageOrderLength = 0;
			AverageProfitOrderLength = 0;
			AverageStopOrderLength = 0;

			if (NumberOfOrders > 0)
			{
				WinPercent = Math.Round(((double)_numberOfWins / NumberOfOrders) * 100.0);
				LossPercent = Math.Round(((double)_numberOfLosses / NumberOfOrders) * 100.0);
				ProfitTargetPercent = Math.Round(((double)_numberOfProfitTargets / NumberOfOrders) * 100.0);
				StopLossPercent = Math.Round(((double)_numberOfStopLosses / NumberOfOrders) * 100.0);
				LengthExceededPercent = Math.Round(((double)_numberOfLengthExceeded / NumberOfOrders) * 100.0);

				AverageOrderLength = Math.Round((double)_totalLengthOfAllOrders / NumberOfOrders);
				AverageProfitOrderLength = _numberOfProfitTargets > 0 ? (double)_totalLengthOfProfitOrders / _numberOfProfitTargets : 0;
				AverageStopOrderLength = _numberOfStopLosses > 0 ? (double)_totalLengthOfStopOrders / _numberOfStopLosses : 0;
			}
		}

		/// <summary>
		/// Inits the values from already calculated statistics.
		/// </summary>
		/// <param name="stats">Other stattistics object.</param>
		public void InitFromStrategyTickerPairStatistics(StrategyTickerPairStatistics stats)
		{
			WinPercent = stats.WinPercent;
			LossPercent = stats.LossPercent;
			ProfitTargetPercent = stats.ProfitTargetPercent;
			StopLossPercent = stats.StopLossPercent;
			LengthExceededPercent = stats.LengthExceededPercent;
			Gain = stats.Gain;
			NumberOfOrders = stats.NumberOfOrders;

			AverageOrderLength = stats.AverageOrderLength;
			AverageProfitOrderLength = stats.AverageProfitOrderLength;
			AverageStopOrderLength = stats.AverageStopOrderLength;
		}
	}
}
