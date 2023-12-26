using CodeFex.NetCore.Data.DateTimeFormat;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeFex.NetCore.Json.Converters
{
    public class JsonDateTimeConverterFactory : JsonConverterFactory
    {
        public DatetimeFormatter DatetimeFormatter { get; protected set; }

        public JsonDateTimeConverterFactory(DatetimeFormatter datetimeFormatter)
        {
            DatetimeFormatter = datetimeFormatter ?? new DatetimeFormatter();
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(DateTime) || typeToConvert == typeof(DateTime?);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(DateTime))
            {
                return new JsonDateTimeConverter(DatetimeFormatter);
            }
            if (typeToConvert == typeof(DateTime?))
            {
                return new JsonDateTimeNullableConverter(DatetimeFormatter);
            }

            return null;
        }
    }

    public class JsonDateTimeNullableConverter : JsonConverter<DateTime?>
    {
        public DatetimeFormatter DatetimeFormatter { get; protected set; }

        public JsonDateTimeNullableConverter(DatetimeFormatter datetimeFormatter)
        {
            DatetimeFormatter = datetimeFormatter ?? new DatetimeFormatter();
        }

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return DatetimeFormatter.TryRead(reader.GetString());
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out var longValue))
                {
                    return DatetimeFormatter.TryRead(longValue);
                }
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                writer.WriteStringValue(DatetimeFormatter.Write(value.Value));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        public DatetimeFormatter DatetimeFormatter { get; protected set; }

        public JsonDateTimeConverter(DatetimeFormatter datetimeFormatter)
        {
            DatetimeFormatter = datetimeFormatter ?? new DatetimeFormatter();
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            DateTime? result = null;

            if (reader.TokenType == JsonTokenType.String)
            {
                result = DatetimeFormatter.TryRead(reader.GetString());
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out var longValue))
                {
                    result = DatetimeFormatter.TryRead(longValue);
                }
            }

            return result != null ? result.Value : DateTime.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(DatetimeFormatter.Write(value));
        }
    }
}
