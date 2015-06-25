using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;
using StockSimulator.Core.BuySellConditions;
using StockSimulator.Indicators;

namespace StockSimulator.Strategies
{
	/// <summary>
	/// </summary>
	public class FibonacciRsi3m3 : RootSubStrategy
	{
		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		/// <param name="factory">Factory for creating dependents</param>
		public FibonacciRsi3m3(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{
		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"BullRsi3m3",
					"BearRsi3m3",
					"FibonacciZones"
				};

				return deps;
			}
		}

		/// <summary>
		/// Returns the name of this strategy.
		/// </summary>
		/// <returns>The name of this strategy</returns>
		public override string ToString()
		{
			return "FibonacciRsi3m3";
		}

		/// <summary>
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			BullRsi3m3 bullStrategy = (BullRsi3m3)Dependents[0];
			BearRsi3m3 bearStrategy = (BearRsi3m3)Dependents[1];

			Strategy foundStrategy = null;
			Strategy oppositeStrategy = null;
			string foundStrategyName = "";

			// See if any setup bars are found.
			if (bullStrategy.WasFound[currentBar] && Data.HigherTimeframeTrend[currentBar] == Order.OrderType.Long)
			{
				// Make sure the signal was found in a fibonacci buy zone.
				if (IsPriceInZone(Data.Open[currentBar], currentBar, Order.OrderType.Long) || IsPriceInZone(Data.Close[currentBar], currentBar, Order.OrderType.Long))
				{
					foundStrategy = bullStrategy;
					oppositeStrategy = bearStrategy;
					foundStrategyName = "BullFibonacciRsi3m3";
				}
			}
			else if (bearStrategy.WasFound[currentBar] && Data.HigherTimeframeTrend[currentBar] == Order.OrderType.Short)
			{
				// Make sure the signal was found in a fibonacci buy zone.
				if (IsPriceInZone(Data.Open[currentBar], currentBar, Order.OrderType.Short) || IsPriceInZone(Data.Close[currentBar], currentBar, Order.OrderType.Short))
				{
					foundStrategy = bearStrategy;
					oppositeStrategy = bullStrategy;
					foundStrategyName = "BearFibonacciRsi3m3";
				}
			}

			if (foundStrategy != null)
			{
				// TODO: use the 1 bar trailing high/low entry conditions.
				List<BuyCondition> buyConditions = new List<BuyCondition>()
				{
					//new AboveSetupBarBuyCondition(Simulator.Config.BressertMaxBarsToFill)
					new MarketBuyCondition()
				};

				// Always have a max time in market and an absolute stop for sell conditions.
				List<SellCondition> sellConditions = new List<SellCondition>()
				{
					new StopSellCondition(0.05),
					new MaxLengthSellCondition(20),
					//new StrategyFoundSellCondition(oppositeStrategy),
				};

				List<string> dependentIndicators = new List<string>()
				{
					"FibonacciZones",
					"Rsi3m3"
				};

				Order placedOrder = EnterOrder(foundStrategyName, currentBar, foundStrategy.OrderType, 10000,
					dependentIndicators, buyConditions, sellConditions);

				if (placedOrder != null)
				{
					// Get things like win/loss percent up to the point this order was started.
					StrategyStatistics orderStats = Simulator.Orders.GetStrategyStatistics(placedOrder.StrategyName,
						placedOrder.Type,
						placedOrder.Ticker.TickerAndExchange,
						currentBar,
						Simulator.Config.MaxLookBackBars);

					Bars[currentBar] = new BarStatistics(
						100.0,
						foundStrategyName,
						foundStrategy.OrderType,
						10000,
						new List<StrategyStatistics>() { orderStats },
						buyConditions,
						sellConditions);
				}
			}
		}

		/// <summary>
		/// Returns if the price is within the timing and price fibonacci zones.
		/// </summary>
		/// <param name="price">Price in question</param>
		/// <param name="currentBar">Bar of the price</param>
		/// <param name="orderType">Type of order</param>
		/// <returns>True if the price is in the timing and price fibonacci zones</returns>
		private bool IsPriceInZone(double price, int currentBar, double orderType)
		{
			FibonacciZones fib = (FibonacciZones)Dependents[2];

			// Check timing first, as long as there is a zone value then it's withing the timing zone.
			if (fib.Zone38[currentBar] > 0.0)
			{
				// See if the price is within the top and bottom zones.
				if (orderType == Order.OrderType.Long && price >= fib.Zone62[currentBar] && price <= fib.Zone38[currentBar])
				{
					return true;
				}
				else if (orderType == Order.OrderType.Short && price >= fib.Zone38[currentBar] && price <= fib.Zone62[currentBar])
				{
					return true;
				}
			}

			return false;
		}
	}
}
