﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;
using Newtonsoft.Json;

namespace StockSimulator.Indicators
{
	/// <summary>
	/// Saves information about the zigzag waves but doesn't plot anything. This is meant
	/// to be used in other indicators.
	/// </summary>
	class ZigZagWaves : Indicator
	{
		/// <summary>
		/// Class used for the bar to hold information about the waves found
		/// before this bar.
		/// </summary>
		public class WaveData
		{
			public WavePoint[] Points { get; set; }
			public double TrendDirection { get; set; }
		}

		/// <summary>
		/// Class that holds a point found on the on the zigzag.
		/// </summary>
		public class WavePoint
		{
			public double Price { get; set; }
			public int Bar { get; set; }
		}


		public List<WaveData> Waves { get; set; }

		#region Configurables
		#endregion

		private int _maxCycleLookback = 100;

		public const int NUMBER_OF_POINTS = 4;
		public const int LAST_POINT = NUMBER_OF_POINTS - 1;


		/// <summary>
		/// Creates the indicator.
		/// Add any dependents here.
		/// </summary>
		/// <param name="tickerData">Price data</param>
		public ZigZagWaves(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				new ZigZag(tickerData, 3.0) { MaxSimulationBars = 150, MaxPlotBars = 150 }
			};

			MaxSimulationBars = 1;
			MaxPlotBars = 0;

			Waves = UtilityMethods.CreateList<WaveData>(Data.NumBars, null);
		}

		/// <summary>
		/// Don't plot on the charts
		/// </summary>
		public override bool HasPlot
		{
			get { return false; }
		}

		/// <summary>
		/// Returns the name of this indicator.
		/// </summary>
		/// <returns>The name of this indicator</returns>
		public override string ToString()
		{
			return "ZigZagWaves";
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar < 2)
			{
				return;
			}

			ZigZag zigzag = (ZigZag)_dependents[0];
			int cutoffBar = Math.Max(0, currentBar - _maxCycleLookback);
			int searchBar = currentBar - 2;
			WavePoint[] points = new WavePoint[NUMBER_OF_POINTS];
			List<double> currentSeries = null;
			int pointBeingSearchFor = LAST_POINT;
			double trendDirection = 0.0;

			// Start searching for the point that either made a high-high or low-low cycle.
			for (int i = searchBar; i >= cutoffBar && pointBeingSearchFor >= 0; i--)
			{
				if (pointBeingSearchFor == LAST_POINT)
				{
					if (zigzag.ZigZagLows[i] > 0.0)
					{
						trendDirection = Order.OrderType.Long;
						points[pointBeingSearchFor] = new WavePoint() { Bar = i, Price = zigzag.Value[i] };
						currentSeries = zigzag.ZigZagHighs;
						--pointBeingSearchFor;
					}
					else if (zigzag.ZigZagHighs[i] > 0.0)
					{
						trendDirection = Order.OrderType.Short;
						points[pointBeingSearchFor] = new WavePoint() { Bar = i, Price = zigzag.Value[i] };
						currentSeries = zigzag.ZigZagLows;
						--pointBeingSearchFor;
					}
				}
				// Each time we find a zigzag point, save the price and move to the next one for searching.
				else if (currentSeries != null && currentSeries[i] > 0.0)
				{
					points[pointBeingSearchFor] = new WavePoint() { Bar = i, Price = zigzag.Value[i] };
					currentSeries = currentSeries == zigzag.ZigZagHighs ? zigzag.ZigZagLows : zigzag.ZigZagHighs;
					--pointBeingSearchFor;
				}
			}

			// Did we get all the points needed to wave information about the waves.
			if (pointBeingSearchFor < 0)
			{
				Waves[currentBar] = new WaveData() { Points = points, TrendDirection = trendDirection };
			}
			else
			{
				Waves[currentBar] = null;
			}
		}

	}
}
