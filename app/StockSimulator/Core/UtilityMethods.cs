﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
			DateTime d1 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan ts = new TimeSpan(dt.Ticks - d1.Ticks);
			return Convert.ToInt64(ts.TotalMilliseconds);
		}

		/// <summary>
		/// Creates a DateTime object from a UNIX time stamp.
		/// </summary>
		/// <param name="timestamp">String of the unix timestamp</param>
		public static DateTime ConvertFromUnixTimestamp(string timestamp)
		{
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			long ms = Convert.ToInt64(timestamp);
			return origin.AddMilliseconds(ms);
		}

		/// <summary>
		/// This presumes that weeks start with Monday. Week 1 is the 1st week of the year with a Thursday in it.
		/// http://stackoverflow.com/questions/11154673/get-the-correct-week-number-of-a-given-date
		/// </summary>
		/// <param name="time">Date to get the week of</param>
		/// <returns>Week index for the year starting with 1</returns>
		public static int GetIso8601WeekOfYear(DateTime time)
		{
			// Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
			// be the same week# as whatever Thursday, Friday or Saturday are,
			// and we always get those right
			DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
			if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
			{
				time = time.AddDays(3);
			}

			// Return the week of our adjusted day
			return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
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
		/// Returns the minimum value of a series from the current bar back to the period value.
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
			return series.GetRange(startIndex, Math.Min(currentBar + 1, period)).Min();
		}

		/// <summary>
		/// Returns the maximum value of a series from the current bar back to the period value.
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
			return series.GetRange(startIndex, Math.Min(currentBar + 1, period)).Max();
		}

		/// <summary>
		/// Returns the percentage change from b to a.
		/// </summary>
		/// <param name="a">Buy price</param>
		/// <param name="b">Sell price</param>
		/// <returns>The percentage change from b to a</returns>
		public static double PercentChange(double a, double b)
		{
			return a > 0.0 ? ((b - a) / a) * 100.0 : 0.0;
		}

		/// <summary>
		/// Checks to see if a valley occured in a data series. A valley is where
		/// the price dips down then comes back up. Will return true for the current
		/// bar which is on the uptick of the valley.
		/// </summary>
		/// <param name="series">Series to test</param>
		/// <param name="currentBar">Current bar to check from</param>
		/// <returns>True if this bar is the end of the valley</returns>
		public static bool IsValley(List<double> series, int currentBar)
		{
			// Not less than equal for the last condition because we want to make sure
			// that we acutally turned up.
			return series[currentBar - 2] >= series[currentBar - 1] && series[currentBar - 1] < series[currentBar];
		}

		/// <summary>
		/// Checks to see if a peak occured in a data series. A peak is where
		/// the price goes up then comes back down. Will return true for the current
		/// bar which is on the downtick of the peak.
		/// </summary>
		/// <param name="series">Series to test</param>
		/// <param name="currentBar">Current bar to check from</param>
		/// <returns>True if this bar is the end of the peak</returns>
		public static bool IsPeak(List<double> series, int currentBar)
		{
			// Not greater than equal for the last condition because we want to make sure
			// that we acutally turned down.
			return series[currentBar - 2] <= series[currentBar - 1] && series[currentBar - 1] > series[currentBar];
		}

		/// <summary>
		/// Compares the two prices to see if b is valued greater than a depending on the trade
		/// direction. So for a long trade, if b is a higher price than a, it is. For a short
		/// trade, if b is a lower price than a, then it is.
		/// </summary>
		/// <param name="a">First price</param>
		/// <param name="b">Second price</param>
		/// <param name="direction">Trade direction, 1.0 for long, -1.0 for short</param>
		/// <returns>Return value > 0: b is valued higher, < 0: b is valued lower, 0: they are equal</returns>
		public static double ComparePrices(double a, double b, double direction)
		{
			// Bull price comparison, is b a higher price than a
			return direction > 0.0 ? b - a : a - b;
		}

		/// <summary>
		/// Calculates the angle of the price vector from b to a and b to c.
		/// </summary>
		/// <param name="a">First point</param>
		/// <param name="b">Second point</param>
		/// <param name="c">Third point</param>
		/// <param name="aBar">First point bar</param>
		/// <param name="bBar">Second point bar</param>
		/// <param name="cBar">Third point bar</param>
		/// <returnsh>The angle in degrees</returns>
		public static double CalculateAngle(double a, double b, double c, int aBar, int bBar, int cBar)
		{
			Point ba = new Point(a - b, aBar - bBar); 
			Point bc = new Point(c - b, cBar - bBar);
			return RadianToDegree(Math.Acos((ba.X * bc.X + ba.Y * bc.Y) / (Math.Sqrt(ba.X * ba.X + ba.Y * ba.Y) * Math.Sqrt(bc.X * bc.X + bc.Y * bc.Y))));
		}

		/// <summary>
		/// Returns the angle in degrees of the slope of the line.
		/// </summary>
		/// <param name="x1">First point x</param>
		/// <param name="y1">First point y</param>
		/// <param name="x2">Second point x</param>
		/// <param name="y2">Second poitn y</param>
		/// <returns>See summary</returns>
		public static double LineAngle(double x1, double y1, double x2, double y2)
		{
			double slope = (y2 - y1) / (x2 - x1);
			return RadianToDegree(Math.Atan(slope));
		}

		/// <summary>
		/// Converts an angle from radians to degrees.
		/// </summary>
		/// <param name="angle">Angle in radians</param>
		/// <returns>Angle in degrees</returns>
		public static double RadianToDegree(double angle)
		{
			return angle * (180.0 / Math.PI);
		}

		/// <summary>
		/// Creates a list with a default size
		/// </summary>
		/// <typeparam name="T">Type of list</typeparam>
		/// <param name="count">Number of items desired</param>
		/// <returns>New list created</returns>
		public static List<T> CreateList<T>(int count)
		{
			if (count < 0)
			{
				throw new ArgumentException("Count of elements cannot be less than zero", "count");
			}

			return new List<T>(new T[count]);
		}

		/// <summary>
		/// Creates a list with a default size and inits it.
		/// </summary>
		/// <typeparam name="T">Type of list</typeparam>
		/// <param name="count">Number of items desired</param>
		/// <param name="val">Value to fill the list with</param>
		/// <returns>New list created</returns>
		public static List<T> CreateList<T>(int count, T val)
		{
			if (count < 0)
			{
				throw new ArgumentException("Count of elements cannot be less than zero", "count");
			}

			List<T> list = new List<T>(new T[count]);
			list.Fill(val);
			return list;
		}

		/// <summary>
		/// Fills a list with a default value which is faster than using Enumerable.Repeat()
		/// </summary>
		/// <typeparam name="T">Type of list</typeparam>
		/// <param name="list">The list being filled</param>
		/// <param name="val">Value to fill the list with</param>
		public static void Fill<T>(this List<T> list, T val)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = val;
			}
		}

		/// <summary>
		/// Returns a new list with the outlier values removed.
		/// http://www.ehow.com/how_5201412_calculate-outliers.html
		/// https://en.wikipedia.org/wiki/Quartile
		/// http://stackoverflow.com/questions/3141692/standard-deviation-of-generic-list
		/// </summary>
		/// <param name="list">List to filter from</param>
		/// <returns>See summary</returns>
		public static void FilterOutliers(this List<int> list)
		{
			if (list.Count() > 0)
			{
				// Compute the Average      
				double avg = list.Average();
				// Perform the Sum of (value-avg)_2_2      
				double sum = list.Sum(d => Math.Pow(d - avg, 2));
				// Put it all together      
				double stddev = Math.Sqrt((sum) / (list.Count() - 1));
				double stddevPlus3 = avg + stddev * 1.1;
				double stddevMinus3 = avg - stddev * 1.1;

				list.RemoveAll(x => x < stddevMinus3 || x > stddevPlus3);
			}
		}

	}
}
