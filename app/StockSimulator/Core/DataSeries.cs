using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// For now just utility functions to compare values.
	/// </summary>
	public class DataSeries
	{
		/// <summary>
		/// Checks if a series crosses over a value anytime in a range of bars.
		/// </summary>
		/// <param name="series1">The series to see if it crosses above a value</param>
		/// <param name="value">Value to see if the series crossed above</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series crossed above. -1 if it didn't cross</returns>
		public static int CrossAbove(List<double> series1, double value, int startBar, int lookBackPeriod)
		{
			int beginBar = startBar - lookBackPeriod;
			for (int i = beginBar; i <= startBar; i++)
			{
				if (series1[i] > value && series1[i - 1] <= value)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Checks if a series crosses over another series anytime in a range of bars.
		/// </summary>
		/// <param name="series1">The first series to see if it crosses above the other series</param>
		/// <param name="series2">The second series to be compared against the first series</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series crossed above. -1 if it didn't cross</returns>
		public static int CrossAbove(List<double> series1, List<double> series2, int startBar, int lookBackPeriod)
		{
			int beginBar = startBar - lookBackPeriod;
			for (int i = beginBar; i <= startBar; i++)
			{
				if (series1[i] > series2[i] && series1[i - 1] <= series2[i - 1])
				{
					return i;
				}
			}
			return -1;
		}




		/// <summary>
		/// Checks if a series crosses below a value anytime in a range of bars.
		/// </summary>
		/// <param name="series1">The series to see if it crosses below a value</param>
		/// <param name="value">Value to see if the series crossed below</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series crossed below. -1 if it didn't cross</returns>
		public static int CrossBelow(List<double> series1, double value, int startBar, int lookBackPeriod)
		{
			int beginBar = startBar - lookBackPeriod;
			for (int i = beginBar; i <= startBar; i++)
			{
				if (series1[i] < value && series1[i - 1] >= value)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Checks if a series crosses below another series anytime in a range of bars.
		/// </summary>
		/// <param name="series1">The first series to see if it crosses below the other series</param>
		/// <param name="series2">The second series to be compared against the first series</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series crossed below. -1 if it didn't cross</returns>
		public static int CrossBelow(List<double> series1, List<double> series2, int startBar, int lookBackPeriod)
		{
			int beginBar = startBar - lookBackPeriod;
			for (int i = beginBar; i <= startBar; i++)
			{
				if (series1[i] < series2[i] && series1[i - 1] >= series2[i - 1])
				{
					return i;
				}
			}
			return -1;
		}
	
		

		/// <summary>
		/// Is a series above a value.
		/// </summary>
		/// <param name="series1">The series get the value for</param>
		/// <param name="value">Value to see if the series is above</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series is above. -1 if it didn't cross</returns>
		public static int IsAbove(List<double> series1, double value, int startBar, int lookBackPeriod)
		{
			int beginBar = startBar - lookBackPeriod;
			for (int i = beginBar; i <= startBar; i++)
			{
				if (series1[i] > value)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Is a series above another series.
		/// </summary>
		/// <param name="series1">The series get the value for</param>
		/// <param name="series2">Series to see if the series is above</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series is above. -1 if it didn't cross</returns>
		public static int IsAbove(List<double> series1, List<double> series2, int startBar, int lookBackPeriod)
		{
			int beginBar = startBar - lookBackPeriod;
			for (int i = beginBar; i <= startBar; i++)
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
		/// <param name="series1">The series get the value for</param>
		/// <param name="value">Value to see if the series crossed below</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series is below. -1 if it's not</returns>
		public static int IsBelow(List<double> series1, double value, int startBar, int lookBackPeriod)
		{
			int beginBar = startBar - lookBackPeriod;
			for (int i = beginBar; i <= startBar; i++)
			{
				if (series1[i] < value)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Is a series below another series.
		/// </summary>
		/// <param name="series1">The series get the value for</param>
		/// <param name="series2">Seriest to see if the other series crossed below</param>
		/// <param name="startBar">The bar to start from</param>
		/// <param name="lookBackPeriod">How many bars to look back from the start bar</param>
		/// <returns>The bar the data series is below. -1 if it's not.</returns>
		public static int IsBelow(List<double> series1, List<double> series2, int startBar, int lookBackPeriod)
		{
			int beginBar = startBar - lookBackPeriod;
			for (int i = beginBar; i <= startBar; i++)
			{
				if (series1[i] < series2[i])
				{
					return i;
				}
			}
			return -1;
		}
	}
}
