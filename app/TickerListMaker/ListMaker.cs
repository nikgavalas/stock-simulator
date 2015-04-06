using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace TickerListMaker
{
	class ListMaker
	{
		private string _inputFileName;
		private string _outputFileName;

		/// <summary>
		/// Initializes the variables of the class.
		/// </summary>
		public ListMaker()
		{
			_inputFileName = "";
			_outputFileName = "";
		}

		/// <summary>
		/// Inits the options from the command line args. If there are invalid args, then throws
		/// an exception.
		/// </summary>
		public void InitFromCommandLine(string[] args)
		{
			string inputOption = "-input:";
			string outputOption = "-output:";

			if (args.Length == 2)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].StartsWith(inputOption))
					{
						_inputFileName = args[i].Substring(inputOption.Length, args[i].Length - inputOption.Length);
					}
					else if (args[i].StartsWith(outputOption))
					{
						_outputFileName = args[i].Substring(outputOption.Length, args[i].Length - outputOption.Length);
					}
				}

				if (_inputFileName == "")
				{
					throw new Exception("No input file command line argument");
				}

				if (_outputFileName == "")
				{
					throw new Exception("No output file command line argument");
				}
			}
			else
			{
				throw new Exception("Invalid number of command line arguments");
			}
		}

		/// <summary>
		/// Creates a list of tickers and exchanges by downloading the list from a server and parsing it.
		/// </summary>
		public void CreateList()
		{
			string[] urls;

			try
			{
				urls = File.ReadAllLines(_inputFileName);
			}
			catch
			{
				throw new Exception("Could not read input file");
			}

			// Download the data for each url and save them in a list to be outputted 
			// to a .csv string buffer then a file.
			List<string> tickersAndExchanges = new List<string>();
			for (int i = 0; i < urls.Length; i++)
			{
				using (WebClient wClient = new WebClient())
				{
					string downloadedData = wClient.DownloadString(urls[i]);
					tickersAndExchanges.AddRange(ParseTickerJson(downloadedData));
				}
			}

			if (tickersAndExchanges.Count == 0)
			{
				throw new Exception("No tickers found from the urls in the config file");
			}

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < tickersAndExchanges.Count; i++)
			{
				sb.AppendLine(tickersAndExchanges[i] + ',');
			}

			// Remove the trailing comma
			string fileData = sb.ToString();
			fileData.TrimEnd(new char[] {','});

			// Output the data to the file.
			File.WriteAllText(_outputFileName, fileData);
		}

		/// <summary>
		/// Parses the json data to return a list of strings that are comma separated
		/// tickers and exchanges like "AAPL,NYSE"
		/// </summary>
		/// <param name="jsonData">json data downloaded from the server</param>
		/// <returns>List of strings of tickers and exchanges</returns>
		private List<string> ParseTickerJson(string jsonData)
		{
			string ticker = "";
			string exchange = "";
			List<string> tickerAndExchanges = new List<string>();

			// Make sure there are no escape characters in the strings.
			jsonData = jsonData.Replace(@"\", "");

			JsonTextReader reader = new JsonTextReader(new StringReader(jsonData));
			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.PropertyName)
				{
					if ((string)reader.Value == "ticker")
					{
						reader.Read();
						ticker = reader.Value.ToString();

						// Get the exchange for this ticker.
						while (reader.Read())
						{
							if (reader.TokenType == JsonToken.PropertyName)
							{
								if ((string)reader.Value == "exchange")
								{
									reader.Read();
									exchange = reader.Value.ToString();
									break;
								}
								else if (reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "ticker")
								{
									throw new Exception("No exchange found for ticker: " + ticker);
								}
							}
						}

						tickerAndExchanges.Add(ticker + "," + exchange);
					}
				}
			}

			return tickerAndExchanges;
		}

	}
}
