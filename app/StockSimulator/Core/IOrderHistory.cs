using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	public interface IOrderHistory
	{
		ConcurrentDictionary<int, List<Order>> TickerDictionary { get; set; }
		ConcurrentDictionary<int, ConcurrentBag<Order>> StrategyDictionary { get; set; }

		/// <summary>
		/// Adds an order to all the dictionaries for searching by multiple key types.
		/// </summary>
		/// <param name="order">The order to add</param>
		/// <param name="dependentIndicators">Indicators used when making a decision to place this order</param>
		/// <param name="currentBar">Current bar the order is being added in</param>
		void AddOrder(Order order, List<Indicator> dependentIndicators, int currentBar);

		/// <summary>
		/// Calculates things like win/loss percent, gain, etc. for the strategy used on the ticker.
		/// </summary>
		/// <param name="strategyName">Name of the strategy the statistics are for</param>
		/// <param name="orderType">Type of orders placed with this strategy (long or short)</param>
		/// <param name="tickerAndExchange">Ticker the strategy used</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <param name="maxBarsAgo">Maximum number of bars in the past to consider for calculating</param>
		/// <returns>Class holding the statistics calculated</returns>
		StrategyStatistics GetStrategyStatistics(string strategyName, double orderType, TickerExchangePair tickerAndExchange, int currentBar, int maxBarsAgo);

		/// <summary>
		/// Calculates things like win/loss percent, gain, etc. for the ticker.
		/// </summary>
		/// <param name="tickerAndExchange">Ticker to calculate for</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <param name="maxBarsAgo">Maximum number of bars in the past to consider for calculating</param>
		/// <returns>Class holding the statistics calculated</returns>
		StrategyStatistics GetTickerStatistics(TickerExchangePair tickerAndExchange, int currentBar, int maxBarsAgo);

		/// <summary>
		/// Frees the orders for a ticker when it finished.
		/// </summary>
		/// <param name="tickerAndExchange">Ticker to free</param>
		void PurgeTickerOrders(TickerExchangePair tickerAndExchange);
	}
}
