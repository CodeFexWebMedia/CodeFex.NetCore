using System.Text.Json;

namespace CodeFex.NetCore.Json.Converters.Format
{
    public class JsonOriginalCaseNamingPolicy : JsonNamingPolicy
    {
        public static readonly JsonOriginalCaseNamingPolicy Instance = new JsonOriginalCaseNamingPolicy();

        public override string ConvertName(string name)
        {
            return name;
        }
    }
}