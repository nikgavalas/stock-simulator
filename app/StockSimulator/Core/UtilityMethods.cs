using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// Class to hold extra methods for various things.
	/// </summary>
	public static class UtilityMethods
	{
		/// <summary>
		/// Returns the number of milliseconds since Jan 1, 1970 (useful for converting C# dates to JS dates) 
		/// </summary>
		/// <param name="dt">DateTime to convert</param>
		/// <returns>Returns the number of milliseconds since Jan 1, 1970</returns>
		public static long UnixTicks(this DateTime dt)
		{
			DateTime d1 = new DateTime(1970, 1, 1);
			DateTime d2 = dt.ToUniversalTime();
			TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
			return Convert.ToInt64(ts.TotalMilliseconds);
		}

		/// <summary>
		/// Calculates a simple moving average of a series.
		/// </summary>
		/// <param name="currentBar">Curent bar of the simulation</param>
		/// <param name="series">The series to get the average from</param>
		/// <param name="period">How many bars to use for the average</param>
		/// <returns>The simple moving average of the series</returns>
		public static double Sma(List<double> series, int currentBar, int period)
		{
			if (currentBar == 0)
			{
				return series[0];
			}

			int avgStartIndex = currentBar - Math.Min(currentBar, period - 1);
			return series.GetRange(avgStartIndex, Math.Min(currentBar, period)).Average();
		}

		/// <summary>
		/// Calculates a exponential moving average of a series.
		/// </summary>
		/// <param name="series">The series to get the average from</param>
		/// <param name="emaSeries">The current ema series</param>
		/// <param name="currentBar">Curent bar of the simulation</param>
		/// <param name="period">How many bars to use for the average</param>
		/// <returns>The exponential moving average of the series</returns>
		public static double Ema(List<double> series, List<double> emaSeries, int currentBar, int period)
		{
			return (currentBar == 0 ? series[0] : series[currentBar] * (2.0 / (1 + period)) + (1 - (2.0 / (1 + period))) * emaSeries[currentBar - 1]);
		}

		/// <summary>
		/// Returns the standard deviation of a series.
		/// </summary>
		/// <param name="currentBar">Curent bar of the simulation</param>
		/// <param name="series">The series to get the min from</param>
		/// <param name="period">How many bars to use for the min</param>
		/// <returns>Standard deviation for the bars</returns>
		public static double StdDev(List<double> series, int currentBar, int period)
		{
			if (currentBar < 1)
			{
				return 0;
			}
			else
			{
				double avg = UtilityMethods.Sma(series, currentBar, period);
				double sum = 0;
				for (int barsBack = Math.Min(currentBar, period - 1); barsBack >= 0; barsBack--)
				{
					sum += (series[currentBar - barsBack] - avg) * (series[currentBar - barsBack] - avg);
				}

				return Math.Sqrt(sum / Math.Min(currentBar + 1, period));
			}
		}

		/// <summary>
		/// Returns the minimum value of a series from the current back back to the period value.
		/// </summary>
		/// <param name="currentBar">Curent bar of the simulation</param>
		/// <param name="series">The series to get the min from</param>
		/// <param name="period">How many bars to use for the min</param>
		/// <returns>Minimum value for the bars</returns>
		public static double Min(List<double> series, int currentBar, int period)
		{
			if (currentBar == 0)
			{
				return series[0];
			}

			int startIndex = currentBar - Math.Min(currentBar, period - 1);
			return series.GetRange(startIndex, Math.Min(currentBar, period)).Min();
		}

		/// <summary>
		/// Returns the maximum value of a series from the current back back to the period value.
		/// </summary>
		/// <param name="currentBar">Curent bar of the simulation</param>
		/// <param name="series">The series to get the max from</param>
		/// <param name="period">How many bars to use for the max</param>
		/// <returns>Maximum value for the bars</returns>
		public static double Max(List<double> series, int currentBar, int period)
		{
			if (currentBar == 0)
			{
				return series[0];
			}

			int startIndex = currentBar - Math.Min(currentBar, period - 1);
			return series.GetRange(startIndex, Math.Min(currentBar, period)).Max();
		}
	}
}
