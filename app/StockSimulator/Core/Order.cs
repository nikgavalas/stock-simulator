using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Core.JsonConverters;
using StockSimulator.Core.BuySellConditions;
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
		public static class OrderType
		{
			public static readonly double Long = 1.0;
			public static readonly double Short = -1.0;
		}

		/// <summary>
		/// Class that defines the different reasons for selling.
		/// </summary>
		public static class SellReasonType
		{
			public static readonly string LengthExceeded = "Length Exceeded";
			public static readonly string ProfitTarget = "Profit Target";
			public static readonly string StopLoss = "Stop Loss";
			public static readonly string StrategyFound = "Strategy Found";
			public static readonly string ForceClose = "Force Closed";
		}

		/// <summary>
		/// The status of the order from it's start to end.
		/// </summary>
		public enum OrderStatus
		{
			Open,
			Filled,
			Closed,
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

		[JsonProperty("sellReason")]
		public string SellReason { get; set; }

		[JsonProperty("orderType")]
		public double Type { get; set; }

		public OrderStatus Status { get; set; }
		public int BuyBar { get; set; }
		public int SellBar { get; set; }
		public double ProfitTargetPrice { get; set; }
		public double StopPrice { get; set; }
		public string StrategyName { get; set; }
		public StrategyStatistics StartStatistics { get; set; }
		public StrategyStatistics EndStatistics { get; set; }
		public List<string> DependentIndicatorNames { get; set; }
		public int OpenedBar { get; set; }

		private double _orderValue;
		private double _sizeOfOrder;

		private List<BuyCondition> _buyConditions;
		private List<SellCondition> _sellConditions;

		/// <summary>
		/// Contructor for the order.
		/// </summary>
		/// <param name="type">Type of order we're placing, long or short</param>
		/// <param name="tickerData">Ticker data</param>
		/// <param name="currentBar">Current bar of the simulation</param>
		/// <param name="sizeOfOrder">Amount of money to place in this order</param>
		/// <param name="fromStrategyName">Name of the strategy this order is for. Can't use the actual strategy reference because it could come from a strategy combo (ie. MacdCrossover-SmaCrossover)</paramparam>
		/// <param name="dependentIndicatorNames">Names of the dependent indicators so they can be shown on the web with the order</param>
		/// <param name="buyConditions">All the buy conditions that must be met to fill the order</param>
		/// <param name="sellConditions">Any of the sell conditions trigger a sell</param>
		public Order(
			double type, 
			TickerData tickerData, 
			string fromStrategyName, 
			int currentBar, 
			double sizeOfOrder,
			List<string> dependentIndicatorNames,
			List<BuyCondition> buyConditions,
			List<SellCondition> sellConditions)
		{
			_orderValue = 0;
			_sizeOfOrder = sizeOfOrder;

			// Save all the buy/sell conditions and sort them so the less
			// desirable conditions get executed last.
			_buyConditions = buyConditions;
			_sellConditions = sellConditions;
			_buyConditions.Sort((a, b) => a.Priority.CompareTo(b.Priority));
			_sellConditions.Sort((a, b) => a.Priority.CompareTo(b.Priority));

			SellReason = "Still open";
			StrategyName = fromStrategyName;
			OrderId = GetUniqueId();
			Type = type;
			Ticker = tickerData;
			DependentIndicatorNames = dependentIndicatorNames;
			AccountValue = 0;
			OpenedBar = currentBar + 1;

			OrderOpened();
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
				double startingValue = NumberOfShares * BuyPrice;
				return ((_orderValue - startingValue) * Type) + startingValue;
			}
		}

		/// <summary>
		/// Updates the order for this bar
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		public void Update(int currentBar)
		{
			// If the order hasn't been filled yet then check if any of the buy 
			// conditions say we should buy. Once one executes the buy, the others
			// are not processed.
			if (Status == OrderStatus.Open)
			{
				for (int i = 0; i < _buyConditions.Count; i++)
				{
					if (_buyConditions[i].OnUpdate(currentBar))
					{
						break;
					}
				}
			}

			// Once the order is filled, check and see if any of the sell conditions
			// say we should sell. Once one executes the sell, the others are not processed.
			if (Status == OrderStatus.Filled)
			{
				_orderValue = NumberOfShares * Ticker.Close[currentBar];

				bool didSell = false;
				for (int i = 0; i < _sellConditions.Count; i++)
				{
					if (_sellConditions[i].OnUpdate(currentBar))
					{
						didSell = true;
						break;
					}
				}

				// Close this order if it would remain open past the simulation date.
				if (didSell == false && currentBar == Ticker.NumBars - 1)
				{
					ForceClose();
				}
			}
		}

		/// <summary>
		/// Buys the order
		/// <param name="buyPrice">Price the order was purchased at</param>
		/// <param name="buyBar">Bar the order was bought on</param>
		/// <param name="buyReason">Reason for buying</param>
		/// </summary>
		public void Buy(double buyPrice, int buyBar, string buyReason)
		{
			BuyPrice = buyPrice;
			BuyBar = buyBar;
			BuyDate = Ticker.Dates[buyBar];
			Status = OrderStatus.Filled;
			NumberOfShares = BuyPrice > 0.0 ? Convert.ToInt32(Math.Floor(_sizeOfOrder / BuyPrice)) : 0;
			
			_orderValue = NumberOfShares * BuyPrice;

			// TODO: this should be moved to a profit or stop sell condition
			//double direction = Type == OrderType.Long ? 1.0 : -1.0;

			//// Set prices to exit.
			//double configProfitTarget = isMainOrder ? Simulator.Config.MainProfitTarget : Simulator.Config.ProfitTarget;
			//double configStopTarget = isMainOrder ? Simulator.Config.MainStopTarget : Simulator.Config.StopTarget;
			//ProfitTargetPrice = BuyPrice + ((BuyPrice * configProfitTarget) * direction);

			//double absoluteStop = BuyPrice - ((BuyPrice * configStopTarget) * direction);
			//if (stopSetAlready == false ||
			//	(stopSetAlready == true && Type == OrderType.Long && StopPrice < absoluteStop) ||
			//	(stopSetAlready == true && Type == OrderType.Short && StopPrice > absoluteStop))
			//{
			//	StopPrice = absoluteStop;
			//}
		}

		/// <summary>
		/// Returns true if this order has closed.
		/// </summary>
		/// <returns>True if this order has closed</returns>
		public bool IsFinished()
		{
			return Status == OrderStatus.Closed;
		}

		/// <summary>
		/// Sells the order.
		/// </summary>
		/// <param name="sellPrice">Price the stock was sold at</param>
		/// <param name="sellBar">Current bar of the simulation</param>
		/// <param name="sellReason">Reason for selling</param>
		public void Sell(double sellPrice, int sellBar, string sellReason)
		{
			// If the sell price is 0 then it's a bug that no more data for this stock exists 
			// and we had an order open. This is not really realistic so we'll just have the order
			// gain $0.
			SellPrice = sellPrice > 0.0 ? sellPrice : BuyPrice;
			SellBar = sellBar;
			SellDate = Ticker.Dates[sellBar];
			Status = OrderStatus.Closed;
			Gain = ((NumberOfShares * SellPrice) - (NumberOfShares * BuyPrice)) * Type;
			Gain -= Simulator.Config.Commission * 2;
			SellReason = sellReason;

			_orderValue = NumberOfShares * SellPrice;
		}

		/// <summary>
		/// Cancels the order.
		/// </summary>
		public void Cancel()
		{
			Status = OrderStatus.Cancelled;
		}

		/// <summary>
		/// Closes the open order with data from the last bar.
		/// </summary>
		private void ForceClose()
		{
			int lastBar = Ticker.NumBars - 1;
			Sell(Ticker.Close[lastBar], lastBar, SellReasonType.ForceClose);
		}

		/// <summary>
		/// Updates the conditions when the order is opened.
		/// </summary>
		private void OrderOpened()
		{
			for (int i = 0; i < _buyConditions.Count; i++)
			{
				_buyConditions[i].OnOpen(this);
			}

			for (int i = 0; i < _sellConditions.Count; i++)
			{
				_sellConditions[i].OnOpen(this);
			}
		}

		/// <summary>
		/// Returns a unique order id to make sure we can lookup orders easily.
		/// </summary>
		private static long _uniqueId = 0;
		private static long GetUniqueId()
		{
			return Interlocked.Increment(ref _uniqueId);
		}

		//public void Update(int curBar)
		//{
		//	bool isMainOrder = GetType() == typeof(MainStrategyOrder);
		//	bool stopSetAlready = false;

		//	// If the order is open and not filled we need to fill it.
		//	if (Status == OrderStatus.Open)
		//	{
		//		// If we are using limit orders make sure the price is higher than that 
		//		// limit before buying.
		//		if (isMainOrder && Simulator.Config.UseLimitOrders)
		//		{
		//			if (curBar - OpenedBar >= Simulator.Config.MaxBarsLimitOrderFill)
		//			{
		//				Status = OrderStatus.Cancelled;
		//			}
		//			else if (Ticker.Open[curBar] >= LimitBuyPrice)
		//			{
		//				BuyPrice = Ticker.Open[curBar];
		//			}
		//			else if (Ticker.Close[curBar] > LimitBuyPrice || Ticker.High[curBar] > LimitBuyPrice)
		//			{
		//				BuyPrice = LimitBuyPrice;
		//			}
		//		}
		//		// Use the last bar and only enter the order if this bar has a higher price than yesterdays
		//		// high. Also, set a cancel/stop order on yesterdays low if the price exceeds that.
		//		else if (curBar > 0 && ((isMainOrder && Simulator.Config.UseOneBarHLMain) || (!isMainOrder && Simulator.Config.UseOneBarHLSub)))
		//		{
		//			if (curBar - OpenedBar >= 1)
		//			{
		//				Status = OrderStatus.Cancelled;
		//			}
		//			else if (Type == OrderType.Long)
		//			{
		//				if (Ticker.Low[curBar] <= Ticker.Low[curBar - 1])
		//				{
		//					Status = OrderStatus.Cancelled;
		//				}
		//				else if (Ticker.High[curBar] >= Ticker.High[curBar - 1])
		//				{
		//					BuyPrice = Ticker.Open[curBar] > Ticker.High[curBar - 1] ? Ticker.Open[curBar] : Ticker.High[curBar - 1];
		//					StopPrice = Ticker.Low[curBar - 1];
		//					stopSetAlready = true;
		//				}
		//			}
		//			else if (Type == OrderType.Short)
		//			{
		//				if (Ticker.High[curBar] >= Ticker.High[curBar - 1])
		//				{
		//					Status = OrderStatus.Cancelled;
		//				}
		//				else if (Ticker.Low[curBar] <= Ticker.Low[curBar - 1])
		//				{
		//					BuyPrice = Ticker.Open[curBar] < Ticker.Low[curBar - 1] ? Ticker.Open[curBar] : Ticker.Low[curBar - 1];
		//					StopPrice = Ticker.High[curBar - 1];
		//					stopSetAlready = true;
		//				}
		//			}
		//		}
		//		else
		//		{
		//			BuyPrice = Ticker.Open[curBar];
		//		}

		//		// Set the stop price to yesterdays low/high depending on order direction.
		//		if (Simulator.Config.UseOneBarHLForStop && !Simulator.Config.UseOneBarHLMain && !Simulator.Config.UseOneBarHLSub)
		//		{
		//			StopPrice = Type == OrderType.Long ? Ticker.Low[curBar - 1] : Ticker.High[curBar - 1];
		//			stopSetAlready = true;
		//		}

		//		if (BuyPrice > 0)
		//		{
		//			BuyBar = curBar;
		//			BuyDate = Ticker.Dates[curBar];
		//			Status = OrderStatus.Filled;

		//			double sizeOfOrder = Simulator.Config.SizeOfOrder;

		//			NumberOfShares = BuyPrice > 0.0 ? Convert.ToInt32(Math.Floor(sizeOfOrder / BuyPrice)) : 0;
		//			_orderValue = NumberOfShares * BuyPrice;

		//			double direction = Type == OrderType.Long ? 1.0 : -1.0;

		//			// Set prices to exit.
		//			double configProfitTarget = isMainOrder ? Simulator.Config.MainProfitTarget : Simulator.Config.ProfitTarget;
		//			double configStopTarget = isMainOrder ? Simulator.Config.MainStopTarget : Simulator.Config.StopTarget;
		//			ProfitTargetPrice = BuyPrice + ((BuyPrice * configProfitTarget) * direction);

		//			double absoluteStop = BuyPrice - ((BuyPrice * configStopTarget) * direction);
		//			if (stopSetAlready == false ||
		//				(stopSetAlready == true && Type == OrderType.Long && StopPrice < absoluteStop) ||
		//				(stopSetAlready == true && Type == OrderType.Short && StopPrice > absoluteStop))
		//			{
		//				StopPrice = absoluteStop;
		//			}
		//		}
		//	}

		//	// Close any orders that need to be closed
		//	if (Status == OrderStatus.Filled)
		//	{
		//		_orderValue = NumberOfShares * Ticker.Close[curBar];

		//		if (Type == OrderType.Long)
		//		{
		//			FinishLongOrder(curBar);
		//		}
		//		else
		//		{
		//			FinishShortOrder(curBar);
		//		}

		//		// Limit the order since we won't want to be in the market forever.
		//		// Also have an option where we can close main orders at a different
		//		// length of bars than substrategy orders.
		//		int numBarsOpen = curBar - BuyBar;
		//		int maxOpenBars = isMainOrder ? Simulator.Config.MaxBarsOrderOpenMain : Simulator.Config.MaxBarsOrderOpen;
		//		if (numBarsOpen >= maxOpenBars && IsFinished() == false)
		//		{
		//			FinishOrder(Ticker.Close[curBar], curBar, OrderStatus.LengthExceeded);
		//		}
		//	}
		//}

		///// <summary>
		///// Returns whether the order is closed.
		///// </summary>
		///// <returns>True if the order is any of the closed statuses</returns>
		//public bool IsFinished()
		//{
		//	return Status == OrderStatus.ProfitTarget ||
		//		Status == OrderStatus.StopTarget ||
		//		Status == OrderStatus.LengthExceeded;
		//}

		///// <summary>
		///// Checks all the conditions that could close a long order and if any
		///// are true then close the order.
		///// </summary>
		///// <param name="curBar">Current bar of the simulation</param>
		//private void FinishLongOrder(int curBar)
		//{
		//	// Gapped open below our stop target, so close at the open price.
		//	if (Ticker.Open[curBar] <= StopPrice)
		//	{
		//		FinishOrder(Ticker.Open[curBar], curBar, OrderStatus.StopTarget);
		//	}
		//	// Either the low or close during this bar was below our stop target,
		//	// then close at the stop target.
		//	else if (Math.Min(Ticker.Close[curBar], Ticker.Low[curBar]) <= StopPrice)
		//	{
		//		FinishOrder(StopPrice, curBar, OrderStatus.StopTarget);
		//	}
		//	// Gapped open above our profit target, then close at the open price.
		//	else if (Ticker.Open[curBar] >= ProfitTargetPrice)
		//	{
		//		FinishOrder(Ticker.Open[curBar], curBar, OrderStatus.ProfitTarget);
		//	}
		//	// Either the high or close during this bar was above our profit target,
		//	// then close at the profit target.
		//	else if (Math.Max(Ticker.Close[curBar], Ticker.High[curBar]) >= ProfitTargetPrice)
		//	{
		//		FinishOrder(ProfitTargetPrice, curBar, OrderStatus.ProfitTarget);
		//	}
		//}

		///// <summary>
		///// Checks all the conditions that could close a short order and if any
		///// are true then close the order.
		///// </summary>
		///// <param name="curBar">Current bar of the simulation</param>
		//private void FinishShortOrder(int curBar)
		//{
		//	// Gapped open above our stop target, so close at the open price.
		//	if (Ticker.Open[curBar] >= StopPrice)
		//	{
		//		FinishOrder(Ticker.Open[curBar], curBar, OrderStatus.StopTarget);
		//	}
		//	// Either the high or close during this bar was above our stop target,
		//	// then close at the stop target.
		//	else if (Math.Max(Ticker.Close[curBar], Ticker.High[curBar]) >= StopPrice)
		//	{
		//		FinishOrder(StopPrice, curBar, OrderStatus.StopTarget);
		//	}
		//	// Gapped open below our profit target, then close at the open price.
		//	else if (Ticker.Open[curBar] <= ProfitTargetPrice)
		//	{
		//		FinishOrder(Ticker.Open[curBar], curBar, OrderStatus.ProfitTarget);
		//	}
		//	// Either the low or close during this bar was below our profit target,
		//	// then close at the profit target.
		//	else if (Math.Min(Ticker.Close[curBar], Ticker.Low[curBar]) <= ProfitTargetPrice)
		//	{
		//		FinishOrder(ProfitTargetPrice, curBar, OrderStatus.ProfitTarget);
		//	}
		//}


	}
}
