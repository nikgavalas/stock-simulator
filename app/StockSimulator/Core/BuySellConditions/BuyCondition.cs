using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core.BuySellConditions
{
	/// <summary>
	/// A buy condition will buy the stock if its parameters are met.
	/// </summary>
	public class BuyCondition
	{
		protected Order _order;

		/// <summary>
		/// Priority of this buy condition, lower = higher
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
		/// Called before the order has been filled during the bar update.
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation for the order</param>
		/// <returns>True if the order was bought</returns>
		public virtual bool OnUpdate(int currentBar)
		{
			return false;
		}
	}
}
