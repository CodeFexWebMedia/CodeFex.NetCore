using System.Text.Json;

namespace CodeFex.NetCore.Json.Converters.Format
{
    public class JsonPascalCaseNamingPolicy : JsonNamingPolicy
    {
        public static readonly JsonPascalCaseNamingPolicy Instance = new JsonPascalCaseNamingPolicy();

        public override string ConvertName(string name)
        {
            if (name != null && name.Length > 0)
            {
                var chars = name.ToCharArray();
                chars[0] = char.ToUpper(chars[0]);

                return new string(chars);
            }

            return name;
        }
    }
}