using CodeFex.NetCore.Json.Serialization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CodeFex.NetCore.Json
{
    public class JsonText
    {
        public static readonly JsonSerializerOptions DefaultSerializerOptions;
        public static readonly JsonWriterOptions JsonWriterOptions;

        static JsonText()
        {
            DefaultSerializerOptions = new JsonSerializerOptions(JsonSerializationDefaults.JsonGenericDefaults) { WriteIndented = true };
            JsonWriterOptions = new JsonWriterOptions() { Indented = true };
        }

        public static string View(object value, bool withName = true, JsonSerializerOptions jsonSerializerOptions = null)
        {
            if (value == null) return null;

            var options = jsonSerializerOptions ?? DefaultSerializerOptions;
            if (options != null && !ReferenceEquals(options, DefaultSerializerOptions))
            {
                // don't change "in use" options
                options = new JsonSerializerOptions(options)
                {
                    WriteIndented = true
                };
            }

            return withName ? string.Concat(value.GetType().Name, ": ", JsonSerializer.Serialize(value, options)) : JsonSerializer.Serialize(value, options);
        }

        public static string PrettyPrint(string json)
        {
            if (json == null || json.Length < 2) return json;

            using (var document = JsonDocument.Parse(json))
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new Utf8JsonWriter(stream, JsonWriterOptions))
                    {
                        document.WriteTo(writer);
                        writer.Flush();

                        var result = Encoding.UTF8.GetString(stream.ToArray());

                        // Regex.Unescape() will generate actual line breaks which are not supported in JSON.
                        return Regex.Unescape(result);
                    }
                }
            }
        }

        public static string PrettyPrint(byte[] bytes)
        {
            if (bytes == null) return null;

            return PrettyPrint(Encoding.UTF8.GetString(bytes));
        }

        public static string PrettyPrint(Stream stream)
        {
            var jsonStream = new MemoryStream();
            stream.CopyTo(jsonStream);

            return PrettyPrint(jsonStream.ToArray());
        }
    }
}
