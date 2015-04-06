using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickerListMaker
{
	/// <summary>
	/// Simple program that downloads tickers from the urls in a config file and then
	/// outputs that list to a .csv file.
	/// </summary>
	class Program
	{
		/// <summary>
		/// Main entry point for the program. Calls everything.
		/// </summary>
		/// <param name="args">Command line args</param>
		static void Main(string[] args)
		{
			ListMaker listMaker = new ListMaker();

			try
			{
				listMaker.InitFromCommandLine(args);
				listMaker.CreateList();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}


	}
}
