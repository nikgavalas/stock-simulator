using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;


namespace StockSimulator.Core
{
	public class SimulatorConfig
	{
		/////////////////////////////// DATES /////////////////////////////////////

		public class DataItemsSource : IItemsSource {
			public ItemCollection GetValues()
			{
				ItemCollection dataTypes = new ItemCollection();
				dataTypes.Add("daily", "Daily");
				dataTypes.Add("minute", "1 Minute");
				dataTypes.Add("twominute", "2 Minute");
				dataTypes.Add("threeminute", "3 Minute");
				dataTypes.Add("fiveminute", "5 Minute");
				return dataTypes;
			}
		}

		[Category("Dates")]
		[DisplayName("Start Date")]
		[Description("Date to start the simulation from")]
		public DateTime StartDate { get; set; }

		[Category("Dates")]
		[DisplayName("End Date")]
		[Description("Date to stop the simulation")]
		public DateTime EndDate { get; set; }

		[Category("Dates")]
		[DisplayName("Use Today For End")]
		[Description("Use today's date for the end date")]
		public bool UseTodaysDate { get; set; }

		[Category("Dates")]
		[DisplayName("Data Type")]
		[Description("Daily, Minute, or 5 Minute data (which is just aggregated from minute data")]
		[ItemsSource(typeof(DataItemsSource))]
		public string DataType { get; set; }

		//////////////////////////// CANDLESTICKS /////////////////////////////////

		[Category("Candlesticks")]
		[DisplayName("Trend Strength")]
		[Description("The number of bars for a trend")]
		public int TrendStrength { get; set; }

		//////////////////////////// HIGHER TIMEFRAME /////////////////////////////

		[Category("Higher Timeframe")]
		[DisplayName("Number Bars")]
		[Description("The number of bars to aggregate the lower timeframe for the higher timeframe")]
		public int NumBarsHigherTimeframe { get; set; }

		[Category("Higher Timeframe")]
		[DisplayName("Use for Substrategies")]
		[Description("Use the higher timeframe momentum on substrategies")]
		public bool UseHigherTimeframeSubstrategies { get; set; }

		[Category("Higher Timeframe")]
		[DisplayName("Use for Main Strategy")]
		[Description("Use the higher timeframe momentum on the main strategy")]
		public bool UseHigherTimeframeMainStrategy { get; set; }

		/////////////////////////////// OUTPUT ////////////////////////////////////

		[Category("Output")]
		[DisplayName("Use Abbreviated Output")]
		[Description("Only outputs the buy list and the overal orders and stats. Significantly improves speed of outputing data.")]
		public bool UseAbbreviatedOutput { get; set; }

		[Category("Output")]
		[DisplayName("Output Last Buy List")]
		[Description("Only outputs the last buy list the occurs on the end date. Useful for finding out what tickers to buy today.")]
		public bool OnlyOutputLastBuyList { get; set; }

		[Category("Output")]
		[DisplayName("Should Open Web Page")]
		[Description("Should auto open the web page after the sim finishes running.")]
		public bool ShouldOpenWebPage { get; set; }

		[Category("Output")]
		[DisplayName("Output Folder")]
		[Description("Folder to output the results to")]
		public string OutputFolder { get; set; }

		//////////////////////////// ALL ORDERS ///////////////////////////////////

		[Category("All Orders")]
		[DisplayName("Max Lookback")]
		[Description("Maximum number of bars to look back when calculating the statistics for the strategy")]
		public int MaxLookBackBars { get; set; }

		[Category("All Orders")]
		[DisplayName("Max Lookback Orders")]
		[Description("Maximum number of orders to look back when calculating the statistics for the strategy")]
		public int MaxLookBackOrders { get; set; }

		[Category("All Orders")]
		[DisplayName("Max Concurrent Orders")]
		[Description("Maximum number of orders that can be for a particular strategy at one time")]
		public int MaxConcurrentOrders { get; set; }

		[Category("All Orders")]
		[DisplayName("Commission Per Trade")]
		[Description("Broker commission per trade. There are two trades per order.")]
		public double Commission { get; set; }

		///////////////////////////// COMBO STRATEGY //////////////////////////////
		
		[Category("Combo Strategy")]
		[DisplayName("Stop Loss Percent")]
		[Description("Stop loss percent for the combo strategy")]
		public double ComboStopPercent { get; set; }

		[Category("Combo Strategy")]
		[DisplayName("Max Bars Open")]
		[Description("Max bars open for the combo strategy")]
		public int ComboMaxBarsOpen { get; set; }

		[Category("Combo Strategy")]
		[DisplayName("Size Of Order")]
		[Description("Amount of money to invest in each stock order")]
		public double ComboSizeOfOrder { get; set; }

		[Category("Combo Strategy")]
		[DisplayName("Combo Leeway")]
		[Description("Number of bars back in time allowed to find a combo from the current bar")]
		public int ComboLeewayBars { get; set; }

		[Category("Combo Strategy")]
		[DisplayName("Min Required Orders")]
		[Description("Number of orders needed before we can use buy signals for a strategy")]
		public int ComboMinRequiredOrders { get; set; }

		[Category("Combo Strategy")]
		[DisplayName("Min Combo Size")]
		[Description("Minimum number of strategies that must have been present to buy")]
		public int ComboMinComboSize { get; set; }

		[Category("Combo Strategy")]
		[DisplayName("Maximum Combo Size")]
		[Description("Maximum size of a combo that can be used")]
		public int ComboMaxComboSize { get; set; }

		[Category("Combo Strategy")]
		[DisplayName("Percent For Buy")]
		[Description("Percent returned from best combo strategy to add ticker to the buy list")]
		public double ComboPercentForBuy { get; set; }

		///////////////////////////// MAIN STRATEGY ///////////////////////////////
		
		[Category("Main Strategy")]
		[DisplayName("Max Orders Per Bar")]
		[Description("Maximum number of orders that can be placed in a bar")]
		public int MaxOrdersPerBar { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Max Open Orders")]
		[Description("Maximum number of orders that can be open at a time")]
		public int MaxOpenOrders { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Main Order Max Bars Open")]
		[Description("Maximum number of bars a main order can be open for")]
		public int MainMaxBarsOrderOpen { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Min Price For Order")]
		[Description("Minimum price for an order to be placed on a ticker")]
		public double MinPriceForOrder { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Min Price For Short")]
		[Description("Minimum price for a short order to be placed on a ticker")]
		public double MinPriceForShort { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Initial Account Balance")]
		[Description("Amount of money the trade account starts with")]
		public int InitialAccountBalance { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Num Bars to Delay Start")]
		[Description("Number of bars to delay purchasing from the buy list")]
		public int NumBarsToDelayStart { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Stock List File")]
		[Description("File with a list of stocks to run the sim on")]
		public string InstrumentListFile { get; set; }

		/// <summary>
		/// Sets the default values for the options.
		/// </summary>
		public SimulatorConfig()
		{
			// Dates
			StartDate = DateTime.Parse("1/4/2010");
			EndDate = DateTime.Parse("3/31/2015");
			DataType = "daily";

			// Candlesticks
			TrendStrength = 4;

			// All orders
			MaxLookBackBars = 500;
			MaxLookBackOrders = 10;
			MaxConcurrentOrders = 1;
			Commission = 4.95;

			// Higher timeframe
			NumBarsHigherTimeframe = 5;
			UseHigherTimeframeSubstrategies = false;
			UseHigherTimeframeMainStrategy = false;

			// Combo strategy
			ComboStopPercent = 0.04;
			ComboMaxBarsOpen = 10;

			// Main strategy
			ComboLeewayBars = 0;
			MaxOrdersPerBar = 4;
			MaxOpenOrders = 1000;
			InitialAccountBalance = 99999999;
			NumBarsToDelayStart = 250;
			MinPriceForOrder = 0.75;
			MinPriceForShort = 5.00;
			InstrumentListFile = @"C:\Users\Nik\Documents\Code\github\stock-simulator\input\test.csv";
			
			// Output
			UseAbbreviatedOutput = true;
			OnlyOutputLastBuyList = false;
			ShouldOpenWebPage = true;
			OutputFolder = @"C:\Users\Nik\Documents\Code\github\stock-simulator\output\output";

			// Testing parameters for indicator correctness.
			//StartDate = DateTime.Parse("12/31/2013");
			//EndDate = DateTime.Parse("12/31/2014");
			//PercentForBuy = 25;
			//MinComboSizeToBuy = 1;
			//MinRequiredOrders = 1;
			//NumBarsToDelayStart = 0;
			//InstrumentListFile = @"C:\Users\Nik\Documents\Code\github\stock-simulator\input\indtest.csv";

		}
	}
}
