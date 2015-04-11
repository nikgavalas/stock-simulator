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

		//////////////////////////// FILTER BAD ///////////////////////////////////

		[Category("Bad Filter")]
		[DisplayName("Should Filter Bad")]
		[Description("Should we filter bad performing stocks in the main strategy")]
		public bool ShouldFilterBad { get; set; }

		[Category("Bad Filter")]
		[DisplayName("Look Back Bars")]
		[Description("How many bars to look back to filter bad stocks")]
		public int NumBarsBadFilter { get; set; }

		[Category("Bad Filter")]
		[DisplayName("Min Profit Target")]
		[Description("The profit target that all orders for a ticker must be above to be considered in the main strategy")]
		public double BadFilterProfitTarget { get; set; }

		//////////////////////////// CANDLESTICKS /////////////////////////////////

		[Category("Candlesticks")]
		[DisplayName("Trend Strength")]
		[Description("The number of bars for a trend")]
		public int TrendStrength { get; set; }

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

		//////////////////////////// ALL ORDERS ///////////////////////////////////

		[Category("Orders")]
		[DisplayName("Min Order Number")]
		[Description("Minimum number of orders a strategy needs for it's statistics to be calculated")]
		public int MinOrders { get; set; }

		[Category("Orders")]
		[DisplayName("Use Bars for Lookback")]
		[Description("If true, use the look back bars for calculating strategy statistics")]
		public bool UseLookbackBars { get; set; }

		[Category("Orders")]
		[DisplayName("Max Lookback")]
		[Description("Maximum number of bars to look back when calculating the statistics for the strategy")]
		public int MaxLookBackBars { get; set; }

		[Category("Orders")]
		[DisplayName("Max Lookback Orders")]
		[Description("Maximum number of orders to look back when calculating the statistics for the strategy")]
		public int MaxLookBackOrders { get; set; }

		[Category("Orders")]
		[DisplayName("Max Concurrent Orders")]
		[Description("Maximum number of orders that can be for a particular strategy at one time")]
		public int MaxConcurrentOrders { get; set; }

		[Category("Orders")]
		[DisplayName("Profit Target")]
		[Description("Profit target for all orders")]
		public double ProfitTarget { get; set; }

		[Category("Orders")]
		[DisplayName("Stop Loss Target")]
		[Description("Stop loss target for all orders")]
		public double StopTarget { get; set; }

		[Category("Orders")]
		[DisplayName("Max Bars Order Open")]
		[Description("Maximum number of bars and order can be open in the market before we close it")]
		public int MaxBarsOrderOpen { get; set; }

		[Category("Orders")]
		[DisplayName("Size Of Order")]
		[Description("Amount of money to invest in each stock order")]
		public double SizeOfOrder { get; set; }

		[Category("Orders")]
		[DisplayName("Use Limit Orders For Buy")]
		[Description("Use a limit order when buying which is equal to the closing price of the previous day")]
		public bool UseLimitOrders { get; set; }

		[Category("Orders")]
		[DisplayName("Max Bars for Limit Order To Fill")]
		[Description("Maximum number of bars to wait for a limit order to fill")]
		public int MaxBarsLimitOrderFill { get; set; }

		[Category("Orders")]
		[DisplayName("Maximum Combo Size")]
		[Description("Maximum size of a combo that can be used")]
		public int MaxComboSize { get; set; }

		///////////////////////////// MAIN STRATEGY ///////////////////////////////

		// Disabled-see the comment in Order.cs
		//[Category("Main Strategy")]
		//[DisplayName("Percent Gain Per Trade")]
		//[Description("Percentage of account value to gain (or lose) on each trade")]
		//public double PercentGainPerTrade { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Combo Leeway")]
		[Description("Number of bars back in time allowed to find a combo from the current bar")]
		public int ComboLeewayBars { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Max Buys Per Bar")]
		[Description("Maximum number of buys that can be made on a single bar")]
		public int MaxBuysPerBar { get; set; }

		//[Category("Main Strategy")]
		//[DisplayName("Max Order Size")]
		//[Description("Maximum amount of money used for a single order")]
		//public double MaxOrderSize { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Min Required Orders")]
		[Description("Number of orders needed before we start counting the statistics as valid")]
		public int MinRequiredOrders { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Min Combo Size")]
		[Description("Minimum number of strategies that must have been present to buy")]
		public int MinComboSizeToBuy { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Initial Account Balance")]
		[Description("Amount of money the trade account starts with")]
		public int InitialAccountBalance { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Percent For Buy")]
		[Description("Percent returned from best of strategy to buy for the main strategy.")]
		public double PercentForBuy { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Num Bars to Delay Start")]
		[Description("Number of bars to delay purchasing from the buy list")]
		public int NumBarsToDelayStart { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Stock List File")]
		[Description("File with a list of stocks to run the sim on")]
		public string InstrumentListFile { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Output Folder")]
		[Description("Folder to output the results to")]
		public string OutputFolder { get; set; }

		public SimulatorConfig()
		{
			StartDate = DateTime.Parse("1/4/2010");
			EndDate = DateTime.Parse("3/31/2015");
			DataType = "daily";

			TrendStrength = 4;

			MaxComboSize = 6;
			MinOrders = 3;
			UseLookbackBars = false;
			MaxLookBackBars = 400;
			MaxLookBackOrders = 10;
			MaxConcurrentOrders = 1;
			ProfitTarget = 0.07;
			StopTarget = 0.04;
			MaxBarsOrderOpen = 15;
			SizeOfOrder = 6000;
			UseLimitOrders = false;
			MaxBarsLimitOrderFill = 3;

			//PercentGainPerTrade = 0.02;
			ComboLeewayBars = 0;
			MinComboSizeToBuy = 1;
			MaxBuysPerBar = 3;
			MinRequiredOrders = 3;
			InitialAccountBalance = 20000;
			PercentForBuy = 80;
			NumBarsToDelayStart = 250;
			//MaxOrderSize = 100000;

			ShouldFilterBad = true;
			NumBarsBadFilter = 400;
			BadFilterProfitTarget = 0.15;

			UseAbbreviatedOutput = true;
			OnlyOutputLastBuyList = true;
			ShouldOpenWebPage = true;

			// Desktop
			InstrumentListFile = @"C:\Users\Nik\Documents\Code\github\stock-simulator\input\test-large.csv";
			// Laptop
			//InstrumentListFile = @"C:\Users\Nik\Documents\github\stock-simulator\input\test.csv";
			// Desktop
			OutputFolder = @"C:\Users\Nik\Documents\Code\github\stock-simulator\output\output";
			// Laptop
			//OutputFolder = @"C:\Users\Nik\Documents\github\stock-simulator\output\output";

			// Testing parameters for indicator correctness.
			//startDate = DateTime.Parse("12/31/2013");
			//endDate = DateTime.Parse("12/31/2014");
			//PercentForBuy = 25;
			//MinComboSizeToBuy = 1;
			//MinRequiredOrders = 1;
			//InstrumentListFile = @"C:\Users\Nik\Documents\Code\github\stock-simulator\input\indtest.csv";

		}
	}
}
