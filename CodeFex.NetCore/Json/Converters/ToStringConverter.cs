using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeFex.NetCore.Json.Converters
{
    public class ToStringConverter : JsonConverter<string>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(string) == typeToConvert;
        }

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }
#if NET
            return Encoding.UTF8.GetString(reader.ValueSpan);
#else
            return Encoding.UTF8.GetString(reader.ValueSpan.ToArray());
#endif
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}