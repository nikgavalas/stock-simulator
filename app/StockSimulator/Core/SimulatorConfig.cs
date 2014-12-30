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

		public SimulatorConfig()
		{
			startDate = DateTime.Parse("1/1/2014");
			endDate = DateTime.Parse("12/31/2014");
		}
	}
}
