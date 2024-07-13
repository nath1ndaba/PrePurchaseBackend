using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrePurchase.Models.Converters
{
    public class TimeOnlyConverter : JsonConverter<TimeOnly>
	{
		public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{

			if (TryGetString(ref reader, out var str))
				return TimeOnly.Parse(str);

			return default;

		}

		private static bool TryGetString(ref Utf8JsonReader reader, out string value)
		{
			try
			{
				value = reader.GetString();
				return true;
			}
			catch
			{
				value = default;
				return false;
			}
		}

		public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
