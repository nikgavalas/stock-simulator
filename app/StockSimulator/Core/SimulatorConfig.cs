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
		[DisplayName("Initial Account Balance")]
		[Description("Amount of money the trade account starts with")]
		public int InitialAccountBalance { get; set; }



		public SimulatorConfig()
		{
			startDate = DateTime.Parse("11/1/2014");
			endDate = DateTime.Parse("12/31/2014");
			MinOrders = 3;
			MaxLookBackBars = 300;
			MaxConcurrentOrders = 1;
			ProfitTarget = 0.03;
			StopTarget = 0.02;
			MaxBarsOrderOpen = 20;
			SizeOfOrder = 10000;

			MaxBuysPerBar = 3;
			InitialAccountBalance = 100000;
		}
	}
}
