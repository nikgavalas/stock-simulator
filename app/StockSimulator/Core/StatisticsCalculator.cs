﻿using System;
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

		// Not serialized!
		public virtual List<Order> Orders { get; set; }

		private int _numberOfWins = 0;
		private int _numberOfLosses = 0;
		private int _numberOfProfitTargets = 0;
		private int _numberOfStopLosses = 0;
		private int _numberOfLengthExceeded = 0;
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
				}
				else if (order.Status == Order.OrderStatus.StopTarget)
				{
					++_numberOfStopLosses;
				}
				else if (order.Status == Order.OrderStatus.LengthExceeded)
				{
					++_numberOfLengthExceeded;
				}

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
			if (NumberOfOrders > 0)
			{
				WinPercent = Math.Round(((double)_numberOfWins / NumberOfOrders) * 100.0);
				LossPercent = Math.Round(((double)_numberOfLosses / NumberOfOrders) * 100.0);
				ProfitTargetPercent = Math.Round(((double)_numberOfProfitTargets / NumberOfOrders) * 100.0);
				StopLossPercent = Math.Round(((double)_numberOfStopLosses / NumberOfOrders) * 100.0);
				LengthExceededPercent = Math.Round(((double)_numberOfLengthExceeded / NumberOfOrders) * 100.0);
			}
		}
	}
}
