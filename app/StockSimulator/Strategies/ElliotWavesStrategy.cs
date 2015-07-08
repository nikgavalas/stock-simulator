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
	public class ElliotWavesStrategy : RootSubStrategy
	{
		private class PriceZone
		{
			public double High { get; set; }
			public double Low { get; set; }
			public int NumberOfPoints { get; set; }
		}

		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		public ElliotWavesStrategy(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				new ElliotWaves(tickerData),
				new PriceRetracements(tickerData),
				new DtOscillator(tickerData) { PeriodRsi = 8, PeriodStoch = 5, PeriodSK = 3, PeriodSD = 3 }
			};
		}

		/// <summary>
		/// Returns the name of this strategy.
		/// </summary>
		/// <returns>The name of this strategy</returns>
		public override string ToString()
		{
			return "ElliotWavesStrategy";
		}

		/// <summary>
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar < 2)
			{
				return;
			}

			ElliotWaves waves = (ElliotWaves)_dependents[0];

			double buyDirection = 0.0;
			string foundStrategyName = "";

			// If we're in a 5th wave, see if the price touched one of the retracement zones.
			if (DataSeries.IsAbove(waves.FifthWaveValue, 0.0, currentBar, 2) != -1 && IsBarInZone(currentBar))
			{
				buyDirection = waves.FifthWaveDirection[currentBar] * -1.0;

				// Verify with the mechanical buy signal.
				DtOscillator dtosc = (DtOscillator)_dependents[2];
				if (buyDirection > 0.0 && Data.HigherTimeframeTrend[currentBar] < 0.0)
				{
					if (dtosc.SD[currentBar] <= 25.0 && dtosc.SK[currentBar] <= 25.0)
					{
						foundStrategyName = "BullElliotWavesStrategy";
					}
				}
				else if (buyDirection < 0.0 && Data.HigherTimeframeTrend[currentBar] > 0.0)
				{
					if (dtosc.SD[currentBar] >= 75.0 && dtosc.SK[currentBar] >= 75.0)
					{
						foundStrategyName = "BearElliotWavesStrategy";
					}
				}
			}

			if (foundStrategyName.Length > 0)
			{
				List<Indicator> dependentIndicators = GetDependentIndicators();

				Order placedOrder = EnterOrder(foundStrategyName, currentBar, buyDirection, 10000,
					dependentIndicators, GetBuyConditions(), GetSellConditions());

				if (placedOrder != null)
				{
					// Get things like win/loss percent up to the point this order was started.
					StrategyStatistics orderStats = Simulator.Orders.GetStrategyStatistics(placedOrder.StrategyName,
						placedOrder.Type,
						placedOrder.Ticker.TickerAndExchange,
						currentBar,
						Simulator.Config.MaxLookBackBars);

					Bars[currentBar] = new OrderSuggestion(
						100.0,
						orderStats.Gain,
						foundStrategyName,
						buyDirection,
						10000,
						dependentIndicators,
						new List<StrategyStatistics>() { orderStats },
						GetBuyConditions(),
						GetSellConditions(),
						null);
				}
			}
		}

		/// <summary>
		/// Checks if the price for this bar falls in a retracement price zone.
		/// </summary>
		/// <param name="currentBar">Bar to check</param>
		/// <returns>True if the price has touched one of the zones</returns>
		private bool IsBarInZone(int currentBar)
		{
			PriceRetracements retracements = (PriceRetracements)_dependents[1];

			// Get the retracement zones first.
			List<PriceZone> zones = new List<PriceZone>();
			List<double> points = new List<double>();
			points.Add(retracements.External[(int)PriceRetracements.ExternalType._127][currentBar]);
			points.Add(retracements.External[(int)PriceRetracements.ExternalType._162][currentBar]);
			points.Add(retracements.Alternate[(int)PriceRetracements.AlternateType._100Wave1][currentBar]);
			points.Add(retracements.Alternate[(int)PriceRetracements.AlternateType._100Wave3][currentBar]);
			points.Add(retracements.Alternate[(int)PriceRetracements.AlternateType._38][currentBar]);
			points.Add(retracements.Alternate[(int)PriceRetracements.AlternateType._62][currentBar]);

			ComboSet<double> comboSet = new ComboSet<double>(points);
			List<List<double>> combos = comboSet.GetSet(2);

			for (int i = 0; i < combos.Count; i++)
			{
				if (AreAllPointsClose(combos[i]) == true)
				{
					double high = combos[i].Max();
					double low = combos[i].Min();
					zones.Add(new PriceZone() { High = high, Low = low, NumberOfPoints = combos[i].Count });
				}
			}

			// The one with the most similar points is the best. See if it lands there first.
			zones.Sort((a, b) => a.NumberOfPoints.CompareTo(b.NumberOfPoints));

			for (int i = 0; i < zones.Count; i++)
			{
				PriceZone zone = zones[i];
				if ((Data.High[currentBar] >= zone.High && Data.Low[currentBar] <= zone.Low) ||
					(Data.High[currentBar] >= zone.Low && Data.Low[currentBar] <= zone.High))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Compares a list of points and if they are all within a certain range returns true.
		/// </summary>
		/// <param name="points">List of points to compare</param>
		/// <returns>True if all points are within a specified percent</returns>
		private bool AreAllPointsClose(List<double> points)
		{
			bool closeEnough = true;

			for (int i = 0; i < points.Count; i++)
			{
				for (int j = i + 1; j < points.Count; j++)
				{
					if (Math.Abs(UtilityMethods.PercentChange(points[i], points[j])) > 2.0)
					{
						closeEnough = false;
						break;
					}
				}
			}

			return closeEnough;
		}

		/// <summary>
		/// Returns the list of buy conditions for this strategy.
		/// </summary>
		/// <returns>List of buy conditions for this strategy</returns>
		private List<BuyCondition> GetBuyConditions()
		{
			return new List<BuyCondition>()
			{
				//new OneBarTrailingHighLow(5)
				new MarketBuyCondition()
			};
		}

		/// <summary>
		/// Returns the list of sell conditions for this strategy.
		/// </summary>
		/// <returns>List of sell conditions for this strategy</returns>
		private List<SellCondition> GetSellConditions()
		{
			// Always have a max time in market and an absolute stop for sell conditions.
			return new List<SellCondition>()
			{
				new StopSellCondition(0.05),
				//new StopOneBarTrailingHighLow(),
				new MaxLengthSellCondition(5),
			};
		}
	
	}
}
