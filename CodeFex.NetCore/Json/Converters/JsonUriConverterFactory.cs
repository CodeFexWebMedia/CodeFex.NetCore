using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeFex.NetCore.Json.Converters
{
    public class JsonUriConverterFactory : JsonConverterFactory
    {
        public JsonUriConverterFactory()
        {
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Uri);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(Uri))
            {
                return new JsonUriConverter();
            }

            return null;
        }
    }

    public class JsonUriConverter : JsonConverter<Uri>
    {
        public override Uri Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null) return null;

            if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var result))
            {
                return result;
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                writer.WriteStringValue(value.ToString());
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
