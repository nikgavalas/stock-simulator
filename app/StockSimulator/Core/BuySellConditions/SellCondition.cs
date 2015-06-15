using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	/// <summary>
	/// Closes the order if the sell condition is true.
	/// </summary>
	public class SellCondition
	{
		protected Order _order;

		/// <summary>
		/// Priority of this sell condition, lower = higher
		/// </summary>
		public virtual int Priority
		{
			get
			{
				return 0;
			}
		}

		/// <summary>
		/// Called when the order is initially opened.
		/// </summary>
		/// <param name="o">The order that was openend</param>
		public virtual void OnOpen(Order o)
		{
			_order = o;
		}

		/// <summary>
		/// Called when the order has been filled during the bar update.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation for the order</param>
		/// <returns>True if the order was closed</returns>
		public virtual bool OnUpdate(int currentBar)
		{
			return false;
		}
	}
}
