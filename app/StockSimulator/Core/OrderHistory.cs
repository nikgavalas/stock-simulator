using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace StockSimulator.Core
{
	/// <summary>
	/// Holds all the orders that are placed from every ticker and strategy. 
	/// Allows lookup for that order by ticker, strategy, or order id
	/// </summary>
	public class OrderHistory : IOrderHistory
	{
		public ConcurrentDictionary<int, List<Order>> TickerDictionary { get; set; }
		public ConcurrentDictionary<int, ConcurrentBag<Order>> StrategyDictionary { get; set; }

		/// <summary>
		/// Initialize the object.
		/// </summary>
		public OrderHistory()
		{
			TickerDictionary = new ConcurrentDictionary<int, List<Order>>();
			StrategyDictionary = new ConcurrentDictionary<int, ConcurrentBag<Order>>();
		}

		/// <summary>
		/// Adds an order to all the dictionaries for searching by multiple key types.
		/// </summary>
		/// <param name="order">The order to add</param>
		/// <param name="currentBar">Current bar the order is being added in</param>
		public void AddOrder(Order order, int currentBar)
		{
			AddToListTable(TickerDictionary, order, order.Ticker.TickerAndExchange.GetHashCode());
			AddToListTableConcurrent(StrategyDictionary, order, order.StrategyName.GetHashCode());
		}

		/// <summary>
		/// Frees the orders for a ticker when it finished.
		/// </summary>
		/// <param name="tickerAndExchange">Ticker to free</param>
		public void PurgeTickerOrders(TickerExchangePair tickerAndExchange)
		{
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

			int tickerHash = tickerAndExchange.GetHashCode();
			if (TickerDictionary.ContainsKey(tickerHash))
			{
				List<Order> tickerOrders = TickerDictionary[tickerHash];

				for (int i = tickerOrders.Count - 1; i >= 0; i--)
				{
					Order order = tickerOrders[i];

					// For the date
					if (order.IsFinished() && order.StrategyName == strategyName &&
						order.BuyBar >= cutoffBar && stats.NumberOfOrders < Simulator.Config.MaxLookBackOrders)
					{ 
						stats.AddOrder(order);
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
			// Orders that started less than this bar will not be considered.
			int cutoffBar = currentBar - maxBarsAgo;
			if (cutoffBar < 0)
			{
				cutoffBar = 0;
			}

			// Order type doesn't matter here since we are just using this class to 
			// output overall ticker info which could be from any order type. It will
			// get ignored on the web output display.
			StrategyStatistics stats = new StrategyStatistics(tickerAndExchange.ToString(), Order.OrderType.Long);

			int tickerHash = tickerAndExchange.GetHashCode();
			if (TickerDictionary.ContainsKey(tickerHash))
			{
				List<Order> tickerOrders = TickerDictionary[tickerHash];

				for (int i = tickerOrders.Count - 1; i >= 0; i--)
				{
					Order order = tickerOrders[i];
					if (order.BuyBar >= cutoffBar)
					{
						stats.AddOrder(order);
					}
				}
			}

			// Only count the statistics if we have a bit more data to deal with.
			// We want to avoid having a strategy say it's 100% correct when it 
			// only has 1 winning trade.
			if (stats.NumberOfOrders > Simulator.Config.MinRequiredOrders)
			{
				stats.CalculateStatistics();
			}
			else
			{
				// For the same reasons as earlier in this function, order type doesn't matter here.
				stats = new StrategyStatistics(tickerAndExchange.ToString(), Order.OrderType.Long);
			}

			return stats;
		}

		/// <summary>
		/// Adds an order to a dictionary that has a list of orders indexed by a string hash.
		/// </summary>
		/// <param name="table">Table to add to</param>
		/// <param name="order">Order to add</param>
		/// <param name="hash">Hash index for the order list</param>
		private void AddToListTable(ConcurrentDictionary<int, List<Order>> table, Order order, int hash)
		{
			if (table.ContainsKey(hash) == false)
			{
				table[hash] = new List<Order>();
			}
			table[hash].Add(order);
		}

		/// <summary>
		/// Adds an order to a dictionary that has a list of orders indexed by a string hash.
		/// </summary>
		/// <param name="table">Table to add to</param>
		/// <param name="order">Order to add</param>
		/// <param name="hash">Hash index for the order list</param>
		private void AddToListTableConcurrent(ConcurrentDictionary<int, ConcurrentBag<Order>> table, Order order, int hash)
		{
			if (table.ContainsKey(hash) == false)
			{
				table[hash] = new ConcurrentBag<Order>();
			}
			table[hash].Add(order);
		}
	}
}
