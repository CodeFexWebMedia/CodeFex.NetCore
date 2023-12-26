using CodeFex.NetCore.Data.Collections.Dynamic;
using CodeFex.NetCore.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeFex.NetCore.Json.Converters
{
    public class JsonDynamicCollectionConverterFactory : JsonConverterFactory
    {
        public static readonly Type IDynamicDataObjectInterface = typeof(IDynamicCollection);

        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert.IsClass && typeToConvert.IsAssignableToInterface(IDynamicDataObjectInterface))
            {
                return true;
            }

            return false;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return (JsonConverter)Activator.CreateInstance(
                    typeof(JsonDynamicCollectionConverter<>).MakeGenericType(typeToConvert),
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    null,
                    culture: null);
        }
    }

    public class JsonDynamicCollectionConverter<T> : JsonConverter<T> where T : DynamicCollection
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert.IsClass && typeToConvert.IsAssignableToInterface(JsonDynamicCollectionConverterFactory.IDynamicDataObjectInterface))
            {
                return true;
            }

            return false;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var startDepth = reader.CurrentDepth;

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var result = (Activator.CreateInstance(typeof(T)) as T).AsDictionary<T>();

                var reading = reader.Read();
                while (reading)
                {
                    string memberIndex;

                    // property
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        memberIndex = reader.GetString();
                        reading = reader.Read();
                    }
                    else
                    {
                        memberIndex = null;
                    }

                    // value
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        result[memberIndex] = reader.GetString();

                        reading = reader.Read();
                        continue;
                    }

                    else if (reader.TokenType == JsonTokenType.Null)
                    {
                        result[memberIndex] = null;

                        reading = reader.Read();
                        continue;
                    }

                    else if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
                    {
                        result[memberIndex] = reader.GetBoolean();

                        reading = reader.Read();
                        continue;
                    }

                    else if (reader.TokenType == JsonTokenType.Number)
                    {
                        if (reader.TryGetInt64(out long value))
                        {
                            result[memberIndex] = value;
                        }
                        else
                        {
                            result[memberIndex] = reader.GetDouble();
                        }

                        reading = reader.Read();
                        continue;
                    }

                    else if (reader.TokenType == JsonTokenType.EndObject || reader.TokenType == JsonTokenType.EndArray)
                    {
                        if (reader.CurrentDepth == startDepth)
                        {
                            return result;
                        }

                        reading = reader.Read();
                        continue;
                    }

                    if (reader.CurrentDepth > startDepth)
                    {
                        var memberValue = Read(ref reader, typeToConvert, options);

#if DEBUG
                        if (memberIndex == null) throw new Exception(string.Concat(nameof(memberIndex), ": ", memberIndex));
                        //Debug.WriteLine(string.Concat(nameof(memberIndex), ": ", memberIndex));
#endif

                        result[memberIndex] = memberValue;
                    }
                }
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                var result = (Activator.CreateInstance(typeof(T)) as T).AsList<T>();

                var memberIndex = DynamicCollection.NewIndex;

                var reading = reader.Read();
                while (reading)
                {
                    // value
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        result[memberIndex] = reader.GetString();

                        reading = reader.Read();
                        continue;
                    }

                    else if (reader.TokenType == JsonTokenType.Null)
                    {
                        result[memberIndex] = null;

                        reading = reader.Read();
                        continue;
                    }

                    else if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
                    {
                        result[memberIndex] = reader.GetBoolean();

                        reading = reader.Read();
                        continue;
                    }

                    else if (reader.TokenType == JsonTokenType.Number)
                    {
                        if (reader.TryGetInt64(out long value))
                        {
                            result[memberIndex] = value;
                        }
                        else
                        {
                            result[memberIndex] = reader.GetDouble();
                        }

                        reading = reader.Read();
                        continue;
                    }

                    else if (reader.TokenType == JsonTokenType.EndArray || reader.TokenType == JsonTokenType.EndObject)
                    {
                        if (reader.CurrentDepth == startDepth)
                        {
                            return result;
                        }

                        reading = reader.Read();
                        continue;
                    }

                    else
                    {
                        var memberValue = Read(ref reader, typeToConvert, options);
                        result[memberIndex] = memberValue;
                    }
                }
            }

            // case when DynamicDataObjectConverter is invoked for DynamicDataObject inside model declaration
            return null;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                if (options.PropertyNamingPolicy == null && options.DictionaryKeyPolicy != null)
                {
                    // propertyNamingPolicy from options as default
                    options.PropertyNamingPolicy = options.DictionaryKeyPolicy;
                }

                var propertyNamingPolicy = options.PropertyNamingPolicy;

                var members = value.Members;

                if (value.IsDictionary)
                {
                    writer.WriteStartObject();
                    foreach (var member in members)
                    {
                        var memberKey = (member as KeyValuePair<string, object>?).Value.Key;
                        var memberValue = (member as KeyValuePair<string, object>?).Value.Value;

                        if (propertyNamingPolicy != null)
                        {
                            writer.WritePropertyName(propertyNamingPolicy.ConvertName(memberKey));
                        }
                        else
                        {
                            writer.WritePropertyName(memberKey);
                        }

                        if (memberValue is T)
                        {
                            Write(writer, memberValue as T, options);
                        }
                        else if (memberValue == null)
                        {
                            writer.WriteNullValue();
                        }
                        else
                        {
                            JsonSerializer.Serialize(writer, memberValue, memberValue.GetType(), options);
                        }
                    }
                    writer.WriteEndObject();
                }
                else if (value.IsList)
                {
                    writer.WriteStartArray();
                    foreach (var item in members)
                    {
                        if (item is T)
                        {
                            Write(writer, item as T, options);
                        }
                        else if (item == null)
                        {
                            writer.WriteNullValue();
                        }
                        else
                        {
                            JsonSerializer.Serialize(writer, item, item.GetType(), options);
                        }
                    }
                    writer.WriteEndArray();
                }
                else if (value.Count == 0)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    JsonSerializer.Serialize(writer, value.Value, value.Value.GetType(), options);
                }
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}