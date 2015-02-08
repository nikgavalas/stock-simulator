using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StockSimulator.Core.JsonConverters
{

	/// <summary>
	/// Rounds the double to 2 places when outputting to json.
	/// </summary>
	public class RoundedDoubleConverter : JsonConverter
	{
		/// <summary>
		/// Writes the double value out.
		/// </summary>
		/// <param name="writer">Json writer</param>
		/// <param name="value">Value of the double</param>
		/// <param name="serializer">Json serializer</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			decimal rounded = 0;
			if (value is double)
			{
				double doubleValue = (Double)value;
				rounded = Math.Round(Convert.ToDecimal(doubleValue), 2);
			}
			else
			{
				throw new NotImplementedException();
			}

			writer.WriteValue(rounded);
		}

		/// <summary>
		/// Lets the converter know if this value can be converted.
		/// </summary>
		/// <param name="objectType">Type of the object being converted</param>
		/// <returns>true if the object can be converted by this converted</returns>
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(double);
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
