using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core.JsonConverters;
using Newtonsoft.Json;

namespace StockSimulator.Core
{
	[JsonObject(MemberSerialization.OptIn)]
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
			LengthExceeded,
			Cancelled
		}

		[JsonProperty("buyPrice")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double BuyPrice { get; set; }
		
		[JsonProperty("sellPrice")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double SellPrice { get; set; }

		[JsonProperty("buyDate")]
		[JsonConverter(typeof(ShortDateTimeConverter))]
		public DateTime BuyDate { get; set; }

		[JsonProperty("sellDate")]
		[JsonConverter(typeof(ShortDateTimeConverter))]
		public DateTime SellDate { get; set; }

		[JsonProperty("numShares")]
		public int NumberOfShares { get; set; }

		[JsonProperty("id")]
		public long OrderId { get; set; }

		[JsonProperty("gain")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double Gain { get; set; }

		[JsonProperty("totalGain")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double TotalGain { get; set; }

		[JsonProperty("accountValue")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double AccountValue { get; set; }

		[JsonProperty("ticker")]
		[JsonConverter(typeof(TickerDataStringConverter))]
		public TickerData Ticker { get; set; }

		public OrderStatus Status { get; set; }
		public int BuyBar { get; set; }
		public int SellBar { get; set; }
		public OrderType Type { get; set; }
		public double ProfitTargetPrice { get; set; }
		public double StopPrice { get; set; }
		public string StrategyName { get; set; }
		public double Value { get; set; }
		public StrategyStatistics StartStatistics { get; set; }
		public StrategyStatistics EndStatistics { get; set; }
		public List<string> DependentIndicatorNames { get; set; }

		private double LimitBuyPrice { get; set; }
		private int LimitOpenedBar { get; set; }

		/// <summary>
		/// Contructor for the order.
		/// </summary>
		/// <param name="tickerData">Ticker data</param>
		public Order(OrderType type, TickerData tickerData, string fromStrategyName, int currentBar, List<string> dependentIndicatorNames)
		{
			StrategyName = fromStrategyName;
			OrderId = GetUniqueId();
			Type = type;
			Ticker = tickerData;
			DependentIndicatorNames = dependentIndicatorNames;
			AccountValue = 0;
			TotalGain = 0;
			LimitBuyPrice = tickerData.Close[currentBar];
			LimitOpenedBar = currentBar;

			// Get things like win/loss percent up to the point this order was finished.
			StartStatistics = Simulator.Orders.GetStrategyStatistics(StrategyName,
				Ticker.TickerAndExchange,
				currentBar,
				Simulator.Config.MaxLookBackBars);
		}

		/// <summary>
		/// Updates the order for this bar
		/// </summary>
		/// <param name="curBar">Current bar of the simulation</param>
		public void Update(int curBar)
		{
			// If the order is open and not filled we need to fill it.
			if (Status == OrderStatus.Open)
			{
				// If we are using limit orders make sure the price is higher than that 
				// limit before buying.
				if (Simulator.Config.UseLimitOrders)
				{
					if (curBar - LimitOpenedBar >= Simulator.Config.MaxBarsLimitOrderFill)
					{
						Status = OrderStatus.Cancelled;
					}
					else if (Ticker.Open[curBar] >= LimitBuyPrice)
					{
						BuyPrice = Ticker.Open[curBar];
					}
					else if (Ticker.Close[curBar] > LimitBuyPrice || Ticker.High[curBar] > LimitBuyPrice)
					{
						BuyPrice = LimitBuyPrice;
					}
				}
				else
				{
					BuyPrice = Ticker.Open[curBar];
				}

				if (BuyPrice > 0)
				{
					BuyBar = curBar;
					BuyDate = Ticker.Dates[curBar];
					Status = OrderStatus.Filled;

					NumberOfShares = BuyPrice > 0.0 ? Convert.ToInt32(Math.Floor(Simulator.Config.SizeOfOrder / BuyPrice)) : 0;
					Value = NumberOfShares * BuyPrice;

					int direction = Type == OrderType.Long ? 1 : -1;

					// Set prices to exit.
					ProfitTargetPrice = BuyPrice + ((BuyPrice * Simulator.Config.ProfitTarget) * direction);
					StopPrice = BuyPrice - ((BuyPrice * Simulator.Config.StopTarget) * direction);
				}
			}

			// Close any orders that need to be closed
			if (Status == OrderStatus.Filled)
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
				if (curBar - BuyBar >= Simulator.Config.MaxBarsOrderOpen)
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
			Gain = Value - (NumberOfShares * BuyPrice);

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
