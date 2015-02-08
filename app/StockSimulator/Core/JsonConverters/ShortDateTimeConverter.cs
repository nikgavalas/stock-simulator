using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StockSimulator.Core.JsonConverters
{
	/// <summary>
	/// Writes the date in a short date format when outputting to json.
	/// </summary>
	class ShortDateTimeConverter : JsonConverter
	{
		/// <summary>
		/// Writes the date value out like "12/01/1999".
		/// </summary>
		/// <param name="writer">Json writer</param>
		/// <param name="value">Value of the date</param>
		/// <param name="serializer">Json serializer</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			string shortDate = "";
			if (value is DateTime)
			{
				shortDate = ((DateTime)value).ToShortDateString();
			}
			else
			{
				throw new NotImplementedException();
			}

			writer.WriteValue(shortDate);
		}

		/// <summary>
		/// Lets the converter know if this value can be converted.
		/// </summary>
		/// <param name="objectType">Type of the object being converted</param>
		/// <returns>true if the object can be converted by this converted</returns>
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(DateTime);
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
	}
}
