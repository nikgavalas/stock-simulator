using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Xceed.Wpf.Toolkit;


namespace StockSimulator.Core
{
	[Serializable]
	public class SimulatorConfig
	{
		[Category("Dates")]
		[DisplayName("Start Date")]
		[Description("Date to start the simulation from")]
		public DateTime startDate { get; set; }

		[Category("Dates")]
		[DisplayName("End Date")]
		[Description("Date to stop the simulation")]
		public DateTime endDate { get; set; }

		//////////////////////////// CANDLESTICKS//////////////////////////////////

		[Category("Candlesticks")]
		[DisplayName("Trend Strength")]
		[Description("The number of bars for a trend")]
		public int TrendStrength { get; set; }

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

		///////////////////////////// MAIN STRATEGY ///////////////////////////////

		[Category("Main Strategy")]
		[DisplayName("Combo Leeway")]
		[Description("Number of bars back in time allowed to find a combo from the current bar")]
		public int ComboLeewayBars { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Max Buys Per Bar")]
		[Description("Maximum number of buys that can be made on a single bar")]
		public int MaxBuysPerBar { get; set; }

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
		[DisplayName("Stock List File")]
		[Description("File with a list of stocks to run the sim on")]
		public string InstrumentListFile { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Output Folder")]
		[Description("Folder to output the results to")]
		public string OutputFolder { get; set; }

		public SimulatorConfig()
		{
			startDate = DateTime.Parse("1/4/2010");
			endDate = DateTime.Parse("12/31/2014");

			TrendStrength = 4;

			MinOrders = 3;
			UseLookbackBars = false;
			MaxLookBackBars = 400;
			MaxLookBackOrders = 10;
			MaxConcurrentOrders = 1;
			ProfitTarget = 0.10;
			StopTarget = 0.05;
			MaxBarsOrderOpen = 30;
			SizeOfOrder = 25000;
			UseLimitOrders = false;
			MaxBarsLimitOrderFill = 3;

			ComboLeewayBars = 1;
			MinComboSizeToBuy = 2;
			MaxBuysPerBar = 5;
			MinRequiredOrders = 5;
			InitialAccountBalance = 100000;
			PercentForBuy = 80;
			// Desktop
			InstrumentListFile = @"C:\Users\Nik\Documents\Code\github\stock-simulator\input\exp-small.csv";
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
			//InstrumentListFile = @"C:\Users\Nik\Documents\Code\github\stock-simulator\input\indtest.csv";

		}
	}
}
