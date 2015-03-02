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

			//double M = 0.0;
			//double S = 0.0;
			//int k = 1;
			//for (int i = currentBar - period; i <= currentBar; i++)
			//{
			//	double value = series[i];
			//	double tmpM = M;
			//	M += (value - tmpM) / k;
			//	S += (value - tmpM) * (value - M);
			//	k++;
			//}
			//return Math.Sqrt(S / (k - 2));
		}
	}
}
