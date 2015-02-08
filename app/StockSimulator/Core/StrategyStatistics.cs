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
	/// Class to hold all the statistics for a strategy.
	/// </summary>
	public class StrategyStatistics
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

		// Not serialized!
		[JsonIgnore]
		public List<Order> Orders { get; set; }

		/// <summary>
		/// Create the class and compute the values.
		/// </summary>
		/// <param name="strategyName">Name of the strategy</param>
		/// <param name="numOrders">Number of orders this strategy produced</param>
		/// <param name="wins">Number of winning trades</param>
		/// <param name="losses">Number of loosing trades</param>
		/// <param name="gain">Amount gained or lost</param>
		public StrategyStatistics(string strategyName, int numOrders, int wins, int losses, double gain)
		{
			StrategyName = strategyName;
			Gain = gain;

			// Calculate the percentages.
			WinPercent = 0;
			LossPercent = 0;
			if (numOrders > 0)
			{
				WinPercent = Math.Round(((double)wins / numOrders) * 100.0);
				LossPercent = Math.Round(((double)losses / numOrders) * 100.0);
			}
		}

		/// <summary>
		/// Constructor that doesn't calculate the stats to be used with add order.
		/// </summary>
		/// <param name="strategyName">Name of the strategy</param>
		public StrategyStatistics(string strategyName)
		{
			StrategyName = strategyName;
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
			}
		}

		/// <summary>
		/// Calculate and save all the statistics.
		/// </summary>
		public void CalculateStatistics()
		{
			int numberOfOrders = 0;
			int numberOfWins = 0;
			int numberOfLosses = 0;
			double totalGain = 0;

			for (int i = 0; i < Orders.Count; i++)
			{
				Order o = Orders[i];
				if (o.IsFinished())
				{
					++numberOfOrders;
					if (o.Gain > 0)
					{
						++numberOfWins;
					}
					else
					{
						++numberOfLosses;
					}

					totalGain += o.Gain;
				}
			}

			Gain = totalGain;
			WinPercent = 0;
			LossPercent = 0;
			if (numberOfOrders > 0)
			{
				WinPercent = Math.Round(((double)numberOfWins / numberOfOrders) * 100.0);
				LossPercent = Math.Round(((double)numberOfLosses / numberOfOrders) * 100.0);
			}
		}
	}
}
