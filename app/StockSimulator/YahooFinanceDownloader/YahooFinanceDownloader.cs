using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.YahooFinanceDownloader
{
	/// <summary>
	/// Downloads daily data from yahoo.
	/// </summary>
	class YahooFinanceUriBuilder
	{
		/// <summary>
		/// Builds a url for do
		/// </summary>
		/// <param name="ticker"></param>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public string GetDailyDataUrl(string ticker, DateTime from, DateTime to)
		{
			string baseUrl = "http://ichart.yahoo.com/table.csv?s={0}&a={1}&b={2}&c={3}&d={4}&e={5}&f={6}&g=d&ignore=.csv";
			return string.Format(baseUrl,
				ticker,
				from.Month - 1,
				from.Day,
				from.Year,
				to.Month - 1,
				to.Day,
				to.Year
			);
		}
	}
}
