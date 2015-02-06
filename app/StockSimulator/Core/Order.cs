using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	[DataContract]
	public class Order
	{
		/// <summary>
		/// Different types of orders that can be placed.
		/// </summary>
		public enum OrderType
		{
			Long,
			Short
		}

		/// <summary>
		/// The status of the order from it's start to end.
		/// </summary>
		public enum OrderStatus
		{
			Open,
			Filled,
			ProfitTarget,
			StopTarget,
			LengthExceeded
		}

		public OrderStatus Status { get; set; }
		public double BuyPrice { get; set; }
		public double SellPrice { get; set; }
		public int BuyBar { get; set; }
		public DateTime BuyDate { get; set; }
		public int SellBar { get; set; }
		public DateTime SellDate { get; set; }
		public OrderType Type { get; set; }
		public double ProfitTargetPrice { get; set; }
		public double StopPrice { get; set; }
		public string StrategyName { get; set; }
		public long OrderId { get; set; }
		public TickerData Ticker { get; set; }
		public int NumberOfShares { get; set; }
		public double Value { get; set; }
		public StrategyStatistics StartStatistics { get; set; }
		public StrategyStatistics EndStatistics { get; set; }

		/// <summary>
		/// Contructor for the order.
		/// </summary>
		/// <param name="tickerData">Ticker data</param>
		public Order(OrderType type, TickerData tickerData, string fromStrategyName, int currentBar)
		{
			StrategyName = fromStrategyName;
			OrderId = GetUniqueId();
			Type = type;
			Ticker = tickerData;

			// Get things like win/loss percent up to the point this order was finished.
			StartStatistics = Simulator.Orders.GetStrategyStatistics(StrategyName,
				Ticker.TickerAndExchange.ToString(),
				currentBar,
				Simulator.Config.MaxLookBackBars);
		}

		/// <summary>
		/// Updates the order for this bar
		/// </summary>
		/// <param name="curBar">Current bar of the simulation</param>
		public void Update(int curBar)
		{
			// TODO: get from config.
			bool UseATR = false;
			double ProfitTarget = 0.03;
			double StopTarget = 0.02;
			int MaxDaysOrderOpen = 20;

			// If the order is open and not filled we need to fill it.
			if (Status == OrderStatus.Open)
			{
				BuyBar = curBar;
				BuyDate = Ticker.Dates[curBar];
				BuyPrice = Ticker.Open[curBar];
				Status = OrderStatus.Filled;

				NumberOfShares = BuyPrice > 0.0 ? Convert.ToInt32(Math.Floor(Simulator.Config.SizeOfOrder / BuyPrice)) : 0;
				Value = NumberOfShares * BuyPrice;

				int direction = Type == OrderType.Long ? 1 : -1;

				// Set prices to exit.
				if (!UseATR)
				{
					ProfitTargetPrice = BuyPrice + ((BuyPrice * ProfitTarget) * direction);
					StopPrice = BuyPrice - ((BuyPrice * StopTarget) * direction);
				}
				else
				{
					// TODO: this won't work yet.
					//ATR atr = mInd.ATR(14);
					//ProfitTargetPrice = BuyPrice + ((atr[curBar] * ProfitTarget) * direction);
					//StopPrice = BuyPrice - ((atr[curBar] * StopTarget) * direction);
				}
			}
			// Close any orders that need to be closed
			else if (Status == OrderStatus.Filled)
			{
				Value = NumberOfShares * Ticker.Close[curBar];

				if (IsMore(Ticker.Open[curBar], ProfitTargetPrice, Type))
				{
					FinishOrder(Ticker.Open[curBar], curBar, OrderStatus.ProfitTarget);
				}
				else if (IsMore(Ticker.Close[curBar], ProfitTargetPrice, Type))
				{
					FinishOrder(ProfitTargetPrice, curBar, OrderStatus.ProfitTarget);
				}
				else if (IsLess(Ticker.Open[curBar], StopPrice, Type))
				{
					FinishOrder(Ticker.Open[curBar], curBar, OrderStatus.StopTarget);
				}
				else if (IsLess(Ticker.Close[curBar], StopPrice, Type))
				{
					FinishOrder(StopPrice, curBar, OrderStatus.StopTarget);
				}

				// Limit the order since we won't want to be in the market forever.
				if (BuyBar - curBar >= MaxDaysOrderOpen)
				{
					FinishOrder(Ticker.Close[curBar], curBar, OrderStatus.LengthExceeded);
				}
			}
		}

		/// <summary>
		/// Returns whether the order is closed.
		/// </summary>
		/// <returns>True if the order is any of the closed statuses</returns>
		public bool IsFinished()
		{
			return Status == OrderStatus.ProfitTarget ||
				Status == OrderStatus.StopTarget ||
				Status == OrderStatus.LengthExceeded;
		}

		/// <summary>
		/// Returns the amount of money this order gained.
		/// </summary>
		/// <returns>The amount of money this order gained</returns>
		public double GetGain()
		{
			// TODO: write this.
			return 0;
		}

		/// <summary>
		/// Util function to return if a is more than b depending on if the order type is for a
		/// positive or negative order.
		/// </summary>
		/// <param name="a">First value</param>
		/// <param name="b">Second value</param>
		/// <param name="type">Type of order</param>
		/// <returns>Returns true if a is valued more than b based on the type of order (long or short)</returns>
		private bool IsMore(double a, double b, OrderType type)
		{
			if (type == OrderType.Long)
			{
				return a >= b;
			}
			else
			{
				return a <= b;
			}
		}

		/// <summary>
		/// Util function to return if a is less than b depending on if the order type is for a
		/// positive or negative order.
		/// </summary>
		/// <param name="a">First value</param>
		/// <param name="b">Second value</param>
		/// <param name="type">Type of order</param>
		/// <returns>Returns true if a is valued less than b based on the type of order (long or short)</returns>
		private bool IsLess(double a, double b, OrderType type)
		{
			if (type == OrderType.Long)
			{
				return a <= b;
			}
			else
			{
				return a >= b;
			}
		}

		/// <summary>
		/// Closes the order and records current stats for orders for this strategy.
		/// </summary>
		/// <param name="sellPrice">Price the stock was sold at</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		private void FinishOrder(double sellPrice, int currentBar, OrderStatus sellStatus)
		{
			SellPrice = sellPrice;
			SellBar = currentBar;
			SellDate = Ticker.Dates[currentBar];
			Status = sellStatus;
			Value = NumberOfShares * SellPrice;

			// Get things like win/loss percent up to the point this order was finished.
			// TODO: not sure if this is needed.
			//EndStatistics = Simulator.Orders.GetStrategyStatistics(StrategyName,
			//	Ticker.TickerAndExchange.ToString(),
			//	currentBar,
			//	Simulator.Config.MaxLookBackBars);
		}

		/// <summary>
		/// Returns a unique order id to make sure we can lookup orders easily.
		/// </summary>
		private static long _uniqueId = 0;
		private long GetUniqueId()
		{
			return ++_uniqueId;
		}
	}
}
