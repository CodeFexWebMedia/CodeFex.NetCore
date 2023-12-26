using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeFex.NetCore.Json.Converters
{
    public class JsonEnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert.IsEnum)
            {
                return true;
            }

            if (typeToConvert.IsGenericType)
            {
                return typeof(Nullable<>) == typeToConvert.GetGenericTypeDefinition() && Nullable.GetUnderlyingType(typeToConvert).IsEnum;
            }

            return false;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter result = null;

            if (typeToConvert.IsEnum)
            {
                result = (JsonConverter)Activator.CreateInstance(
                    typeof(JsonEnumConverter<>).MakeGenericType(typeToConvert),
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    null,
                    culture: null);
            }

            if (typeToConvert.IsGenericType)
            {
                result = (JsonConverter)Activator.CreateInstance(
                    typeof(JsonNullableEnumConverter<>).MakeGenericType(Nullable.GetUnderlyingType(typeToConvert)),
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    null,
                    culture: null);
            }

            return result;
        }
    }

    public class JsonEnumConverter<T> : JsonConverter<T> where T : struct
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null) return default;

            if (Enum.TryParse<T>(value, true, out var result))
            {
                return result;
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            string keyValue;

            if (options != null && options.PropertyNamingPolicy != null)
            {
                keyValue = options.PropertyNamingPolicy.ConvertName(value.ToString());
            }
            else
            {
                keyValue = value.ToString();
            }

            if (keyValue != null)
            {
                writer.WriteStringValue(keyValue);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    public class JsonNullableEnumConverter<T> : JsonConverter<T?> where T : struct
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null) return null;

            /*
            if (Enum.TryParse(Nullable.GetUnderlyingType(typeToConvert), value, true, out var result))
            {
                return result as T?;
            }
            */

            if (Enum.TryParse<T>(value, true, out var result))
            {
                return result;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            string keyValue = null;

            if (value != null)
            {
                if (options != null && options.PropertyNamingPolicy != null)
                {
                    keyValue = options.PropertyNamingPolicy.ConvertName(value.Value.ToString());
                }
                else
                {
                    keyValue = value.Value.ToString();
                }
            }

            if (keyValue != null)
            {
                writer.WriteStringValue(keyValue);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
