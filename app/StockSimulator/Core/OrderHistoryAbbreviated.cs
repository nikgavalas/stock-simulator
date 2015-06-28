using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// Holds all the orders but in a very short history. Only enough to get the
	/// stats needed to run the sim. Doesn't hold enough stats to output
	/// all the order history for analyzing.
	/// </summary>
	public class OrderHistoryAbbreviated : IOrderHistory
	{
		public ConcurrentDictionary<int, Dictionary<int, List<Order>>> TickerStrategyOrders;

		public ConcurrentDictionary<int, ConcurrentBag<Order>> StrategyDictionary { get; set; }
		public ConcurrentDictionary<int, List<Order>> TickerDictionary { get; set; }

		/// <summary>
		/// Initialize the object.
		/// </summary>
		public OrderHistoryAbbreviated()
		{
			TickerDictionary = new ConcurrentDictionary<int, List<Order>>();
			StrategyDictionary = new ConcurrentDictionary<int, ConcurrentBag<Order>>();
			TickerStrategyOrders = new ConcurrentDictionary<int, Dictionary<int, List<Order>>>();
		}

		/// <summary>
		/// Adds an order to all the dictionaries for searching by multiple key types.
		/// </summary>
		/// <param name="order">The order to add</param>
		/// <param name="dependentIndicators">Indicators used when making a decision to place this order</param>
		/// <param name="currentBar">Current bar the order is being added in</param>
		public void AddOrder(Order order, List<Indicator> dependentIndicators, int currentBar)
		{
			// Save the main order in the regular strategy dictionary since we want to 
			// save all of it's orders with no weird processing.
			if (order.GetType() == typeof(MainStrategyOrder))
			{
				int mainKey = order.StrategyName.GetHashCode();
				if (StrategyDictionary.ContainsKey(mainKey) == false)
				{
					StrategyDictionary[mainKey] = new ConcurrentBag<Order>();
				}

				StrategyDictionary[mainKey].Add(order);
			}
			else
			{
				// If this is the first time we've seen this ticker, need a new strategy dictionary 
				// for this ticker.
				int tickerKey = order.Ticker.TickerAndExchange.GetHashCode();
				if (TickerStrategyOrders.ContainsKey(tickerKey) == false)
				{
					TickerStrategyOrders[tickerKey] = new Dictionary<int, List<Order>>();
				}

				Dictionary<int, List<Order>> tickerDictionary = TickerStrategyOrders[tickerKey];

				// If this is the first time we've seen this strategy for this ticker, create a 
				// new list of orders to track it.
				int strategyKey = order.StrategyName.GetHashCode();
				if (tickerDictionary.ContainsKey(strategyKey) == false)
				{
					tickerDictionary[strategyKey] = new List<Order>();
				}

				// Remove any old orders before adding the new one to the list.
				List<Order> orders = tickerDictionary[strategyKey];
				RemoveOldOrders(orders, currentBar);
				orders.Add(order);
			}
		}

		/// <summary>
		/// Saves the indicator series for this order so that during analysis we can see
		/// exactly what the indicators looked like at the time the order was placed.
		/// </summary>
		/// <param name="order">The order to add</param>
		/// <param name="dependentIndicators">Indicators used when making a decision to place this order</param>
		/// <param name="currentBar">Current bar the order was placed.</param>
		public void SaveSnapshot(Order order, List<Indicator> dependentIndicators, int currentBar)
		{
			// Only care about the main orders for the abbreviated output.
			if (order.StrategyName == "MainStrategy")
			{
				Simulator.DataOutput.OutputIndicatorSnapshots(order, dependentIndicators, currentBar);
			}
		}

		/// <summary>
		/// Frees the orders for a ticker when it finished.
		/// </summary>
		/// <param name="tickerAndExchange">Ticker to free</param>
		public void PurgeTickerOrders(TickerExchangePair tickerAndExchange)
		{
			TickerStrategyOrders[tickerAndExchange.GetHashCode()] = null;
		}

		/// <summary>
		/// Calculates things like win/loss percent, gain, etc. for the strategy used on the ticker.
		/// </summary>
		/// <param name="strategyName">Name of the strategy the statistics are for</param>
		/// <param name="orderType">Type of orders placed with this strategy (long or short)</param>
		/// <param name="tickerAndExchange">Ticker the strategy used</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <param name="maxBarsAgo">Maximum number of bars in the past to consider for calculating</param>
		/// <returns>Class holding the statistics calculated</returns>
		public StrategyStatistics GetStrategyStatistics(string strategyName, double orderType, TickerExchangePair tickerAndExchange, int currentBar, int maxBarsAgo)
		{
			// Orders that started less than this bar will not be considered.
			int cutoffBar = currentBar - maxBarsAgo;
			if (cutoffBar < 0)
			{
				cutoffBar = 0;
			}

			StrategyStatistics stats = new StrategyStatistics(strategyName, orderType);

			int tickerKey = tickerAndExchange.GetHashCode();
			if (TickerStrategyOrders.ContainsKey(tickerKey))
			{
				Dictionary<int, List<Order>> tickerDictionary = TickerStrategyOrders[tickerKey];

				int strategyKey = strategyName.GetHashCode();
				if (tickerDictionary.ContainsKey(strategyKey))
				{
					List<Order> tickerOrders = tickerDictionary[strategyKey];

					for (int i = tickerOrders.Count - 1; i >= 0; i--)
					{
						Order order = tickerOrders[i];

						// Add orders that are newer than the maximum lookback and only keep a set
						// amount of orders.
						if (order.IsFinished() && order.BuyBar >= cutoffBar &&
							stats.NumberOfOrders < Simulator.Config.MaxLookBackOrders)
						{
							stats.AddOrder(order);
						}
					}
				}
			}

			if (stats.NumberOfOrders > Simulator.Config.MinRequiredOrders)
			{
				stats.CalculateStatistics();
			}
			else
			{
				stats = new StrategyStatistics(strategyName, orderType);
			}

			return stats;
		}

		/// <summary>
		/// Calculates things like win/loss percent, gain, etc. for the ticker.
		/// </summary>
		/// <param name="tickerAndExchange">Ticker to calculate for</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <param name="maxBarsAgo">Maximum number of bars in the past to consider for calculating</param>
		/// <returns>Class holding the statistics calculated</returns>
		public StrategyStatistics GetTickerStatistics(TickerExchangePair tickerAndExchange, int currentBar, int maxBarsAgo)
		{
			// TODO: add way to calculate this.
			return new StrategyStatistics(tickerAndExchange.ToString(), Order.OrderType.Long);
		}

		/// <summary>
		/// Removes any orders that are too old to be used to calculate statistics from.
		/// </summary>
		/// <param name="orders">Array of orders</param>
		/// <param name="currentBar">Current bar to base the age from</param>
		private void RemoveOldOrders(List<Order> orders, int currentBar)
		{
			// Remove all the orders that come before this current bar.
			int cutoffBar = currentBar - Simulator.Config.MaxLookBackBars;
			if (cutoffBar < 0)
			{
				cutoffBar = 0;
			}

			orders.RemoveAll(o => o.BuyBar < cutoffBar);

			// Remove any ones that are greater than the maximum number.
			while (orders.Count > Simulator.Config.MaxLookBackOrders)
			{
				orders.RemoveAt(0);
			}
		}
	}
}
