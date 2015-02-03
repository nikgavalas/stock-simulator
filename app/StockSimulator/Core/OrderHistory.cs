using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// Holds all the orders that are placed from every ticker and strategy. 
	/// Allows lookup for that order by ticker, strategy, or order id
	/// </summary>
	public class OrderHistory
	{
		public Dictionary<long, Order> IdDictionary { get; set; }
		public Dictionary<int, List<Order>> TickerDictionary { get; set; }
		public Dictionary<int, List<Order>> StrategyDictionary { get; set; }

		/// <summary>
		/// Initialize the object.
		/// </summary>
		public OrderHistory()
		{
			IdDictionary = new Dictionary<long, Order>();
			TickerDictionary = new Dictionary<int, List<Order>>();
			StrategyDictionary = new Dictionary<int, List<Order>>();
		}

		/// <summary>
		/// Adds an order to all the dictionaries for searching by multiple key types.
		/// </summary>
		/// <param name="order">The order to add</param>
		public void AddOrder(Order order)
		{
			if (IdDictionary.ContainsKey(order.OrderId))
			{
				throw new Exception("Duplicate order ids found");	
			}
			
			IdDictionary[order.OrderId] = order;
			AddToListTable(TickerDictionary, order, order.Ticker.TickerAndExchange.GetHashCode());
			AddToListTable(StrategyDictionary, order, order.StrategyName.GetHashCode());
		}

		/// <summary>
		/// Calculates things like win/loss percent, gain, etc. for the strategy used on the ticker.
		/// </summary>
		/// <param name="strategyName">Name of the strategy the statistics are for</param>
		/// <param name="tickerName">Ticker the strategy used</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <param name="maxBarsAgo">Maximum number of bars in the past to consider for calculating</param>
		/// <returns>Class holding the statistics calculated</returns>
		public StrategyStatistics GetStrategyStatistics(string strategyName, string tickerName, int currentBar, int maxBarsAgo)
		{
			// Orders that started less than this bar will not be considered.
			int cutoffBar = currentBar - maxBarsAgo;
			if (cutoffBar < 0)
			{
				cutoffBar = 0;
			}

			int numberOfWins = 0;
			int numberOfLosses = 0;
			int numberOfOrders = 0;
			double totalGain = 0;

			List<Order> strategyOrders = StrategyDictionary[strategyName.GetHashCode()];
			for (int i = 0; i < strategyOrders.Count; i++)
			{
				Order order = strategyOrders[i];
				if (order.BuyBar >= cutoffBar && order.IsFinished())
				{
					++numberOfOrders;

					double gain = order.GetGain();
					totalGain += gain;
					if (gain >= 0)
					{
						++numberOfWins;
					}
					else
					{
						++numberOfLosses;
					}
				}
			}

			StrategyStatistics stats = new StrategyStatistics(
				strategyName,
				numberOfOrders,
				numberOfWins,
				numberOfLosses,
				totalGain);

			return stats;
		}

		/// <summary>
		/// Adds an order to a dictionary that has a list of orders indexed by a string hash.
		/// </summary>
		/// <param name="table">Table to add to</param>
		/// <param name="order">Order to add</param>
		/// <param name="hash">Hash index for the order list</param>
		private void AddToListTable(Dictionary<int, List<Order>> table, Order order, int hash)
		{
			if (table.ContainsKey(hash) == false)
			{
				table[hash] = new List<Order>();
			}
			table[hash].Add(order);
		}
	}
}
