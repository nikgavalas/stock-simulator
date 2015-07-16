using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StockSimulator.Core
{
	/// <summary>
	/// Our simulated broker which really just handles our money.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class Broker
	{
		[JsonProperty("accountValue")]
		public List<List<object>> AccountValue { get; set; }
		
		public double AccountCash { get; set; }
		public double CurrentAccountValue { get; set; }

		/// <summary>
		/// Initializes the broker.
		/// </summary>
		/// <param name="startingCash">Cash we start our sim with</param>
		/// <param name="numberOfBars">Number of bars we'll have in the sim</param>
		public Broker(double startingCash, int numberOfBars)
		{
			AccountCash = startingCash;
			CurrentAccountValue = startingCash;
			AccountValue = new List<List<object>>();
		}

		/// <summary>
		/// Adds account value for the day so we can graph it and track it later.
		/// </summary>
		/// <param name="date">Date of the value</param>
		/// <param name="value">Account value on this date</param>
		public void AddValueToList(DateTime date, double value)
		{
			AccountValue.Add(new List<object>()
			{
				UtilityMethods.UnixTicks(date),
				Math.Round(value, 2)
			});
		}
	}
}
