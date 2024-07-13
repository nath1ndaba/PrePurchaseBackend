using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrePurchase.Models.Converters
{
    public class DateTimeConverter : JsonConverter<DateTime>
	{
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
            if (TryGetString(ref reader, out var str))
            {
                if (str != null)
                {
                    return DateTime.Parse(str);
                }
                else
                {
                    // Handle null string case here
                    // For example, throw an exception or return a default value
                }
            }

            return default;

		}

		internal static bool TryGetString(ref Utf8JsonReader reader, out string value)
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

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}

	public class NullableDateTimeConverter : JsonConverter<DateTime?>
	{
		public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{

			if (DateTimeConverter.TryGetString(ref reader, out var str))
				return DateTime.Parse(str);

			return null;

		}

		public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.HasValue ? value.ToString() : "");
		}
	}
}
