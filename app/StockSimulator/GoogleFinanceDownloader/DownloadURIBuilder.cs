using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace StockSimulator.GoogleFinanceDownloader {
	/// <summary>
	/// Class used to create the URLs that identify which data must be downloaded.
	/// </summary>
	public class DownloadURIBuilder {
		#region Constants
		/// <summary>
		/// Length of a one-day interval.
		/// </summary>
		private const int INTERVAL_TO_DOWNLOAD_HISTORICAL_DATA_WITH_GETPRICES = 24 * 3600; //24 hours and 3600 seconds per hour.
		#endregion
		
		#region Class fields
		/// <summary>
		/// Name of the exchange where the ticker is defined.
		/// If it's null or empty, Google assumes it must be
		/// an American exchange.
		/// </summary>
		private readonly string Exchange;

		/// <summary>
		/// Name of the ticker.
		/// </summary>
		private readonly string TickerName;
		#endregion

		#region Initialization
		/// <summary>
		/// The exchange and the ticker are defined here.
		/// </summary>        
		public DownloadURIBuilder(string exchange, string tickerName) {            
			if (String.IsNullOrEmpty(tickerName))
				throw new ArgumentException("Can't be null or empty.", "exchange");
			Exchange = exchange;
			TickerName = tickerName;
		}
		#endregion

		#region URI generator methods
		/// <summary>
		/// Calls getGetPricesUri with Interval = one day and Period = the number of years
		/// since 1970.        
		/// </summary>
		/// <param name="lastDate">Must be the current date. DateTime.Now isn't used to
		/// avoid dependencies with the system time.</param>        
		public string getGetPricesUrlToDownloadAllData(DateTime lastDate) {
			return getGetPricesUri(INTERVAL_TO_DOWNLOAD_HISTORICAL_DATA_WITH_GETPRICES, getPeriodToDownloadAllData(lastDate));
		}
		
		/// <summary>
		/// Calls getGetPricesUri with Interval = 1 day and Period = the number of 
		/// days between 'startDate' and 'endDate'. The aim of this method is to return 
		/// at least the data between 'startDate' and today. Although the ending date is fixed (Google
		/// doesn't allow to define it), it is passed as an argument in order to avoid dependencies 
		/// between the library code and the current system time. 
		/// 
		/// Evidently, this method's URL returns data prior to 'startDate'. To avoid this, things 
		/// like the the number of holidays between 'startDate' and 'endDate' should be taken into account. 
		/// It seems that that would overcomplicate things for this simple example. 
		/// </summary>        
		public string getGetPricesUrlForRecentData(DateTime startDate, DateTime endDate) {
			return getGetPricesUri(INTERVAL_TO_DOWNLOAD_HISTORICAL_DATA_WITH_GETPRICES, getPeriod(startDate, endDate));
		}


		/// <summary>
		/// Defines the interval as a minute and the period one day.
		/// </summary>        
		public string getGetPricesUrlForLastQuote() {
			return getGetPricesUri(60, "1d");
		}
		
		/// <summary>
		/// Returns a URI to invoke the getPrices method.
		/// </summary>
		public string getGetPricesUri(int interval, string period) {
			if (String.IsNullOrEmpty(period))
				throw new ArgumentException("No 'period'.");

			string formatURI = START_GET_PRICES_URI + "?q={0}{1}&i={2}&p={3}&f=d,c,h,l,o,v";

			string exchangeString = string.Empty;
			if (!String.IsNullOrEmpty(Exchange)) {
				exchangeString = "&x=" + Exchange;
			}

			return String.Format(formatURI, TickerName, exchangeString, interval, period);
		}
		#endregion

		#region Auxiliary methods
		/// <summary>
		/// Returns the beginning of the URI to download
		/// the last quote.
		/// </summary>
		protected String START_GET_PRICES_URI {
			get {
				return "http://www.google.com/finance/getprices";
			}
		}
		
		/// <summary>
		/// Returns the period value that downloads all data. 
		/// </summary>
		/// <returns></returns>
		private string getPeriodToDownloadAllData(DateTime lastDate) {
			int year = lastDate.Year;            
			return (year - 1970) + "Y";
		}

		/// <summary>
		/// Method invoked by getGetPricesUriForRecentData().
		/// </summary>
		private string getPeriod(DateTime startDate, DateTime endDate) {
			if (endDate.Date < startDate.Date) {
				throw new ArgumentException("The ending date can't be lower than the starting date.");                
			}

			if (endDate.Year > startDate.Year) { //More than a year.
				return (endDate.Year - startDate.Year + 1) + "Y";
			}

			TimeSpan span = endDate - startDate;

			if (span.Days > 50) { //Return months.
				return ((span.Days % 25) + 1) + "M";
			}
			else { //Return days.
				return (span.Days + 1) + "d"; //+1 because, depending on the hour, no data might be retrieved (1d is for today). 
			}
		}
		#endregion

	}
}
