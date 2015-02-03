using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	public class Strategy : Runnable
	{
		/// <summary>
		/// Holds whether the strategy had a buy signal for each bar.
		/// </summary>
		public List<bool> WasFound { get; set; }

		/// <summary>
		/// The type of orders this strategy places.
		/// </summary>
		protected Order.OrderType _orderType;

		/// <summary>
		/// List of all the orders that are not closed.
		/// </summary>
		private List<Order> _activeOrders { get; set; }

		/// <summary>
		/// Constructor to initialize the strategy.
		/// </summary>
		/// <param name="tickerData">Data for the ticker involved</param>
		/// <param name="factory">Factory to create dependent runnables</param>
		public Strategy(TickerData tickerData, RunnableFactory factory) : base(tickerData, factory)
		{
			WasFound = Enumerable.Repeat(false, tickerData.NumBars).ToList();
			_activeOrders = new List<Order>();
			_orderType = Order.OrderType.Long; // Default to long orders.
		}

		/// <summary>
		/// Updates the orders that are from this strategy.
		/// </summary>
		/// <param name="currentBar">The current bar in the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			// Update all the open orders.
			for (int i = 0; i < _activeOrders.Count; i++)
			{
				Order order = _activeOrders[i];
				order.Update(currentBar);
			}

			// Remove the orders that are finished. This will just remove them from
			// this array but they order will still be saved in the order history.
			_activeOrders.RemoveAll(order => order.IsFinished());
		}

		/// <summary>
		/// Creates and enters an order depending on the type of orders this strategy places (Long or Short)
		/// </summary>
		/// <param name="strategyName">Name of the strategy that placed the order</param>
		/// <param name="currentBar">Bar the order was placed on</param>
		/// <returns>The order that was placed or null if none was placed</returns>
		protected Order EnterOrder(string strategyName, int currentBar)
		{
			Order order = null;

			// Find the number of open orders this strategy current has.
			int openOrders = 0;
			for (int i = 0; i < _activeOrders.Count; i++)
			{
				if (_activeOrders[i].StrategyName == strategyName)
				{
					++openOrders;
				}
			}

			// Only place the order if it's less than the allowed amount of concurrent orders allowed.
			if (openOrders < Simulator.Config.MaxConcurrentOrders)
			{
				order = new Order(_orderType, Data, strategyName, currentBar);
				Simulator.Orders.AddOrder(order);
				_activeOrders.Add(order);
			}

			return order;
		}

	}
}
