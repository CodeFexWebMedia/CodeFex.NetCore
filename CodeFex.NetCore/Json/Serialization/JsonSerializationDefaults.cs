using CodeFex.NetCore.Data.DateTimeFormat;
using CodeFex.NetCore.Json.Converters;
using CodeFex.NetCore.Json.Converters.Format;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace CodeFex.NetCore.Json.Serialization
{
    public class JsonSerializationDefaults
    {
        public static JsonSerializerOptions JsonGenericDefaults
        {
            get
            {
                var options = new JsonSerializerOptions(JsonSerializerDefaults.General);

                var datetimeFormatter = new DatetimeFormatter(DateTimeFormat.Iso8601_Simple);

                // input/output options
                options.Converters.Add(new JsonDateTimeConverterFactory(datetimeFormatter));
                options.Converters.Add(new JsonEnumConverterFactory());
                options.Converters.Add(new JsonUriConverterFactory());
                options.Converters.Add(new JsonDynamicCollectionConverterFactory());

                // output options
                options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
                options.PropertyNameCaseInsensitive = true;
                options.PropertyNamingPolicy = JsonOriginalCaseNamingPolicy.Instance;
                options.DictionaryKeyPolicy = JsonOriginalCaseNamingPolicy.Instance;

                options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

                options.WriteIndented = true;

                return options;
            }
        }

        public static JsonSerializerOptions JsonApiDefaults
        {
            get
            {
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

                var datetimeFormatter = new DatetimeFormatter(DateTimeFormat.Iso8601_Simple);

                // input/output options
                options.Converters.Add(new JsonDateTimeConverterFactory(datetimeFormatter));
                options.Converters.Add(new JsonEnumConverterFactory());
                options.Converters.Add(new JsonUriConverterFactory());
                options.Converters.Add(new JsonDynamicCollectionConverterFactory());

                // output options
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.PropertyNameCaseInsensitive = true;
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

                options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

                options.WriteIndented = false;

                return options;
            }
        }
    }
}
