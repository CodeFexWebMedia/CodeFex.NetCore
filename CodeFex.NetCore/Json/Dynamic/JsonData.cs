using CodeFex.NetCore.Data.Collections.Dynamic;

namespace CodeFex.NetCore.Json.Dynamic
{
    public class JsonData : DynamicCollection
    {
        public JsonData From(string key, object value)
        {
            this[key] = value;

            return this;
        }

        public static JsonData As(string key, object value)
        {
            return new JsonData().AsDictionary<JsonData>().From(key, value);
        }
    }
}
