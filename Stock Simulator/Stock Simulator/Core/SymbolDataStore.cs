using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	/// <summary>
	/// Returns data for the symbol. If the symbol isn't in memory or doesn't
	/// exist, then we get it from the ol' internet thing.
	/// </summary>
	public class SymbolDataStore
	{
		SortedDictionary<string, SymbolData> _symbolsInMemory;

		/// <summary>
		/// Constructor
		/// </summary>
		public SymbolDataStore()
		{
			_symbolsInMemory = new SortedDictionary<string, SymbolData>();
		}

		/// <summary>
		/// Gets the symbol data from either memory, disk, or a server.
		/// </summary>
		/// <param name="symbolCode">Symbol code to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the symbol code</returns>
		public SymbolData GetSymbolData(string symbolCode, DateTime start, DateTime end)
		{
			SymbolData data = new SymbolData();

			// The symbol exists in memory already.
			if (_symbolsInMemory.ContainsKey(symbolCode))
			{
				data = _symbolsInMemory[symbolCode];

				// Make sure the symbol has data for the dates that we want
				// If it doesn't then we have to get some more data.
				if (data.Start > start || data.End < end)
				{
					data = GetDataFromDiskOrServer(symbolCode, start, end);
				}

			}
			// Symbol isn't in memory so we need to load from the disk or the server.
			else
			{
				data = GetDataFromDiskOrServer(symbolCode, start, end);
			}
			return data;
		}

		/// <summary>
		/// Tries to get the data from the disk first. If all the data isn't on disk
		/// then request it from the server.
		/// </summary>
		/// <param name="symbolCode">Symbol code to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the symbol code</returns>
		private SymbolData GetDataFromDiskOrServer(string symbolCode, DateTime start, DateTime end)
		{
			SymbolData data = GetDataFromDisk(symbolCode, start, end);

			// Check if the data from the disk is in the range. If it's
			// not then we need to get data from the server so we have it.
			if (data == null || data.Start > start || data.End < end)
			{
				data = GetDataFromServer(symbolCode, start, end);
			}

			return data;
		}

		/// <summary>
		/// Gets the data from the disk for the symbol dates requested. If it doesn't exist
		/// on disk, then we'll have to get it from the internet and then save it on disk
		/// for later use.
		/// </summary>
		/// <param name="symbolCode">Symbol code to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the symbol code</returns>
		private SymbolData GetDataFromDisk(string symbolCode, DateTime start, DateTime end)
		{
			// If the file doesn't exist then we for sure have to pull it from the internet.
			// If file != exist
			return null;

			// If file exists
			// Load it into memory
			// Make sure that we have all the dates we need
			// If not all dates


		}

		/// <summary>
		/// Gets the data from the webserver and saves it onto disk for later usage.
		/// </summary>
		/// <param name="symbolCode">Symbol code to get data for</param>
		/// <param name="start">Start date for the data</param>
		/// <param name="end">End date for the data</param>
		/// <returns>Data (price, volume, etc) for the symbol code</returns>
		private SymbolData GetDataFromServer(string symbolCode, DateTime start, DateTime end)
		{
			return null;
		}
	}
}
