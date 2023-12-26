using System.Collections.Generic;

namespace CodeFex.NetCore.Json.Dynamic
{
    public static class JsonDataArrayExtensions
    {
        public static string[] ToStringArray(this JsonDataArray jsonDataArray)
        {
            if (jsonDataArray == null) return null;

            var result = new List<string>();
            foreach (var member in jsonDataArray.Members)
            {
                if (member != null)
                {
                    result.Add(member.ToString());
                }
                else
                {
                    result.Add(null);
                }
            }

            return result.ToArray();
        }
    }

    public class JsonDataArray : JsonData
    {
        public JsonDataArray()
        {
            AsList<JsonDataArray>();
        }
    }
}
