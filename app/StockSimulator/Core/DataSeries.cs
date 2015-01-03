using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	public class DataSeries
	{
		/// <summary>
		/// Rewrite of NinjaTrader's CrossAbove to allow for starting from a frame and looking back from there.
		/// </summary>
		/// <param name="series1">The series get the value for.</param>
		/// <param name="value">Value to see if the series crossed above</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series crossed above. -1 if it didn't cross</returns>
		public int CrossAbove(IDataSeries series1, double value, int startBar, int lookBackPeriod)
		{
			int endBar = startBar + lookBackPeriod;
			for (int i = startBar; i < endBar; i++)
			{
				if (series1[i] > value && series1[i + 1] <= value)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Rewrite of NinjaTrader's CrossAbove to allow for starting from a frame and looking back from there.
		/// </summary>
		/// <param name="series1">The series get the value for.</param>
		/// <param name="value">Value to see if the series crossed above</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series crossed above. -1 if it didn't cross</returns>
		public int CrossAbove(IDataSeries series1, IDataSeries series2, int startBar, int lookBackPeriod)
		{
			int endBar = startBar + lookBackPeriod;
			for (int i = startBar; i < endBar; i++)
			{
				if (series1[i] > series2[i] && series1[i + 1] <= series2[i + 1])
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Rewrite of NinjaTrader's CrossBelow to allow for starting from a frame and looking back from there.
		/// </summary>
		/// <param name="series1">The series get the value for.</param>
		/// <param name="value">Value to see if the series crossed below</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series crossed below. -1 if it didn't cross</returns>
		public int CrossBelow(IDataSeries series1, double value, int startBar, int lookBackPeriod)
		{
			int endBar = startBar + lookBackPeriod;
			for (int i = startBar; i < endBar; i++)
			{
				if (series1[i] < value && series1[i + 1] >= value)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Rewrite of NinjaTrader's CrossBelow to allow for starting from a frame and looking back from there.
		/// </summary>
		/// <param name="series1">The series get the value for.</param>
		/// <param name="value">Value to see if the series crossed below</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series crossed below. -1 if it didn't cross</returns>
		public int CrossBelow(IDataSeries series1, IDataSeries series2, int startBar, int lookBackPeriod)
		{
			int endBar = startBar + lookBackPeriod;
			for (int i = startBar; i < endBar; i++)
			{
				if (series1[i] < series2[i] && series1[i + 1] >= series2[i + 1])
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Is a series above a value.
		/// </summary>
		/// <param name="series1">The series get the value for.</param>
		/// <param name="value">Value to see if the series is above</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series is above. -1 if it didn't cross</returns>
		public int IsAbove(IDataSeries series1, double value, int startBar, int lookBackPeriod)
		{
			int endBar = startBar + lookBackPeriod;
			for (int i = startBar; i < endBar; i++)
			{
				if (series1[i] > value)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Is a series above a value.
		/// </summary>
		/// <param name="series1">The series get the value for.</param>
		/// <param name="series2">Series to see if the series is above</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series is above. -1 if it didn't cross</returns>
		public int IsAbove(IDataSeries series1, IDataSeries series2, int startBar, int lookBackPeriod)
		{
			int endBar = startBar + lookBackPeriod;
			for (int i = startBar; i < endBar; i++)
			{
				if (series1[i] > series2[i])
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Is a series below a value.
		/// </summary>
		/// <param name="series1">The series get the value for.</param>
		/// <param name="value">Value to see if the series crossed below</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series is below. -1 if it's not</returns>
		public int IsBelow(IDataSeries series1, double value, int startBar, int lookBackPeriod)
		{
			int endBar = startBar + lookBackPeriod;
			for (int i = startBar; i < endBar; i++)
			{
				if (series1[i] < value)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Is a series below a value.
		/// </summary>
		/// <param name="series1">The series get the value for.</param>
		/// <param name="value">Value to see if the series crossed below</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series is below. -1 if it's not.</returns>
		public int IsBelow(IDataSeries series1, IDataSeries series2, int startBar, int lookBackPeriod)
		{
			int endBar = startBar + lookBackPeriod;
			for (int i = startBar; i < endBar; i++)
			{
				if (series1[i] < series2[i])
				{
					return mInd.CurrentBar - i;
				}
			}
			return -1;
		}

	}
}
