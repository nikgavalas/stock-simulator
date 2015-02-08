using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StockSimulator.Core.JsonConverters
{
	class TickerDataStringConverter : JsonConverter
	{
		/// <summary>
		/// Writes the ticker data object as just a string name of the ticker.
		/// </summary>
		/// <param name="writer">Json writer</param>
		/// <param name="value">Value of the ticker</param>
		/// <param name="serializer">Json serializer</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			string tickerString = "";
			if (value is TickerData)
			{
				tickerString = ((TickerData)value).TickerAndExchange.ToString();
			}
			else
			{
				throw new NotImplementedException();
			}

			writer.WriteValue(tickerString);
		}

		/// <summary>
		/// Lets the converter know if this value can be converted.
		/// </summary>
		/// <param name="objectType">Type of the object being converted</param>
		/// <returns>true if the object can be converted by this converted</returns>
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(TickerData);
		}

		/// <summary>
		/// Not implemented yet since we only output.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="objectType"></param>
		/// <param name="existingValue"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Read is not implemented
		/// </summary>
		public override bool CanRead
		{
			get { return false; }
		}
	}
}
