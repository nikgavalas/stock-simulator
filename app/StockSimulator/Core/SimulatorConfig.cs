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

		//////////////////////////// ALL ORDERS ///////////////////////////////////

		[Category("Orders")]
		[DisplayName("Min Order Number")]
		[Description("Minimum number of orders a strategy needs for it's statistics to be calculated")]
		public int MinOrders { get; set; }

		[Category("Orders")]
		[DisplayName("Max Lookback")]
		[Description("Maximum number of bars to look back when calculating the statistics for the strategy")]
		public int MaxLookBackBars { get; set; }

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

		///////////////////////////// MAIN STRATEGY ///////////////////////////////
	
		[Category("Main Strategy")]
		[DisplayName("Max Buys Per Bar")]
		[Description("Maximum number of buys that can be made on a single bar")]
		public int MaxBuysPerBar { get; set; }

		[Category("Main Strategy")]
		[DisplayName("Min Required Orders")]
		[Description("Number of orders needed before we start counting the statistics as valid")]
		public int MinRequiredOrders { get; set; }

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

		public SimulatorConfig()
		{
			startDate = DateTime.Parse("1/1/2010");
			endDate = DateTime.Parse("12/31/2014");
			// Test dates for indicator correctness.
			//startDate = DateTime.Parse("12/31/2013");
			//endDate = DateTime.Parse("12/31/2014");
			MinOrders = 3;
			MaxLookBackBars = 400;
			MaxConcurrentOrders = 1;
			ProfitTarget = 0.05;
			StopTarget = 0.04;
			MaxBarsOrderOpen = 15;
			SizeOfOrder = 10000;

			MaxBuysPerBar = 3;
			MinRequiredOrders = 3;
			InitialAccountBalance = 100000;
			PercentForBuy = 70;
			InstrumentListFile = @"C:\Users\Nik\Documents\Code\github\stock-simulator\input\nasdaq3.csv";
		}
	}
}
