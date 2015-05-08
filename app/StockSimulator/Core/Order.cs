using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading;

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

		[JsonProperty("accountValue")]
		[JsonConverter(typeof(RoundedDoubleConverter))]
		public double AccountValue { get; set; }

		[JsonProperty("ticker")]
		[JsonConverter(typeof(TickerDataStringConverter))]
		public TickerData Ticker { get; set; }

		[JsonProperty("orderStatus")]
		public OrderStatus Status { get; set; }

		[JsonProperty("orderType")]
		public OrderType Type { get; set; }

		public int BuyBar { get; set; }
		public int SellBar { get; set; }
		public double ProfitTargetPrice { get; set; }
		public double StopPrice { get; set; }
		public string StrategyName { get; set; }
		public StrategyStatistics StartStatistics { get; set; }
		public StrategyStatistics EndStatistics { get; set; }
		public List<string> DependentIndicatorNames { get; set; }

		private double LimitBuyPrice { get; set; }
		private int LimitOpenedBar { get; set; }

		private double _orderValue;

		/// <summary>
		/// Contructor for the order.
		/// </summary>
		/// <param name="type">Type of order we're placing, long or short</param>
		/// <param name="tickerData">Ticker data</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <param name="fromStrategyName">Name of the strategy this order is for. Can't use the actual strategy reference because it could come from a strategy combo (ie. MacdCrossover-SmaCrossover)</paramparam>
		/// <param name="dependentIndicatorNames">Names of the dependent indicators so they can be shown on the web with the order</param>
		public Order(OrderType type, TickerData tickerData, string fromStrategyName, int currentBar, List<string> dependentIndicatorNames)
		{
			_orderValue = 0;

			StrategyName = fromStrategyName;
			OrderId = GetUniqueId();
			Type = type;
			Ticker = tickerData;
			DependentIndicatorNames = dependentIndicatorNames;
			AccountValue = 0;
			LimitBuyPrice = tickerData.Close[currentBar];
			LimitOpenedBar = currentBar;

			// Get things like win/loss percent up to the point this order was finished.
			StartStatistics = Simulator.Orders.GetStrategyStatistics(StrategyName,
				type,
				Ticker.TickerAndExchange,
				currentBar,
				Simulator.Config.MaxLookBackBars);
		}

		/// <summary>
		/// Returns the value of the order as positive regardless of a long or short order.
		/// Ex. If we short order bought 10 shares at $100, then after 3 bars the value price
		/// of the stock is $90. The value of the order is actually $1100 because we made 
		/// $10 per share since we shorted it. Long orders are much easier to visualize.
		/// The reason for this is so that in our account value we always store how much of
		/// an overall value our portfolio is and we want that to account for short orders too.
		/// </summary>
		public double Value
		{
			get
			{
				double direction = Type == OrderType.Long ? 1.0 : -1.0;
				double startingValue = NumberOfShares * BuyPrice;
				return ((_orderValue - startingValue) * direction) + startingValue;
			}
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

					double sizeOfOrder = Simulator.Config.SizeOfOrder;

					NumberOfShares = BuyPrice > 0.0 ? Convert.ToInt32(Math.Floor(sizeOfOrder / BuyPrice)) : 0;
					_orderValue = NumberOfShares * BuyPrice;

					double direction = Type == OrderType.Long ? 1.0 : -1.0;

					// Set prices to exit.
					ProfitTargetPrice = BuyPrice + ((BuyPrice * Simulator.Config.ProfitTarget) * direction);
					StopPrice = BuyPrice - ((BuyPrice * Simulator.Config.StopTarget) * direction);
				}
			}

			// Close any orders that need to be closed
			if (Status == OrderStatus.Filled)
			{
				_orderValue = NumberOfShares * Ticker.Close[curBar];

				if (Type == OrderType.Long)
				{
					FinishLongOrder(curBar);
				}
				else
				{
					FinishShortOrder(curBar);
				}

				// Limit the order since we won't want to be in the market forever.
				// Also have an option where we can close main orders at a different
				// length of bars than substrategy orders.
				int numBarsOpen = curBar - BuyBar;
				bool shouldForceCloseOrder = (numBarsOpen >= Simulator.Config.MaxBarsOrderOpen) ||
					(GetType() == typeof(MainStrategyOrder) && numBarsOpen >= Simulator.Config.MaxBarsOrderOpenMain);
				if (shouldForceCloseOrder)
				{
					// We'll simulate it so that if we are holding for 0 bars (opening and 
					// closing in 1 day), then we'll just sell at the end of the day ignoring
					// profit targets but heeding stop loss targets.
					if (IsFinished() == false || (GetType() == typeof(MainStrategyOrder) && Simulator.Config.MaxBarsOrderOpenMain == 0 && Status == OrderStatus.ProfitTarget))
					{
						FinishOrder(Ticker.Close[curBar], curBar, OrderStatus.LengthExceeded);
					}
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
		/// Checks all the conditions that could close a long order and if any
		/// are true then close the order.
		/// </summary>
		/// <param name="curBar">Current bar of the simulation</param>
		private void FinishLongOrder(int curBar)
		{
			// Gapped open below our stop target, so close at the open price.
			if (Ticker.Open[curBar] <= StopPrice)
			{
				FinishOrder(Ticker.Open[curBar], curBar, OrderStatus.StopTarget);
			}
			// Either the low or close during this bar was below our stop target,
			// then close at the stop target.
			else if (Math.Min(Ticker.Close[curBar], Ticker.Low[curBar]) <= StopPrice)
			{
				FinishOrder(StopPrice, curBar, OrderStatus.StopTarget);
			}
			// Gapped open above our profit target, then close at the open price.
			else if (Ticker.Open[curBar] >= ProfitTargetPrice)
			{
				FinishOrder(Ticker.Open[curBar], curBar, OrderStatus.ProfitTarget);
			}
			// Either the high or close during this bar was above our profit target,
			// then close at the profit target.
			else if (Math.Max(Ticker.Close[curBar], Ticker.High[curBar]) >= ProfitTargetPrice)
			{
				FinishOrder(ProfitTargetPrice, curBar, OrderStatus.ProfitTarget);
			}
		}

		/// <summary>
		/// Checks all the conditions that could close a short order and if any
		/// are true then close the order.
		/// </summary>
		/// <param name="curBar">Current bar of the simulation</param>
		private void FinishShortOrder(int curBar)
		{
			// Gapped open above our stop target, so close at the open price.
			if (Ticker.Open[curBar] >= StopPrice)
			{
				FinishOrder(Ticker.Open[curBar], curBar, OrderStatus.StopTarget);
			}
			// Either the high or close during this bar was above our stop target,
			// then close at the stop target.
			else if (Math.Min(Ticker.Close[curBar], Ticker.High[curBar]) >= StopPrice)
			{
				FinishOrder(StopPrice, curBar, OrderStatus.StopTarget);
			}
			// Gapped open below our profit target, then close at the open price.
			else if (Ticker.Open[curBar] <= ProfitTargetPrice)
			{
				FinishOrder(Ticker.Open[curBar], curBar, OrderStatus.ProfitTarget);
			}
			// Either the low or close during this bar was below our profit target,
			// then close at the profit target.
			else if (Math.Max(Ticker.Close[curBar], Ticker.Low[curBar]) <= ProfitTargetPrice)
			{
				FinishOrder(ProfitTargetPrice, curBar, OrderStatus.ProfitTarget);
			}
		}

		/// <summary>
		/// Closes the order and records current stats for orders for this strategy.
		/// </summary>
		/// <param name="sellPrice">Price the stock was sold at</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		private void FinishOrder(double sellPrice, int currentBar, OrderStatus sellStatus)
		{
			double direction = Type == OrderType.Long ? 1.0 : -1.0;
			
			// If the sell price is 0 then it's a bug that no more data for this stock exists 
			// and we had an order open. This is not really realistic so we'll just have the order
			// gain $0.
			SellPrice = sellPrice > 0.0 ? sellPrice : BuyPrice;
			SellBar = currentBar;
			SellDate = Ticker.Dates[currentBar];
			Status = sellStatus;
			Gain = ((NumberOfShares * SellPrice) - (NumberOfShares * BuyPrice)) * direction;

			_orderValue = NumberOfShares * SellPrice;

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
		private static long GetUniqueId()
		{
			return Interlocked.Increment(ref _uniqueId);
		}
	}
}
