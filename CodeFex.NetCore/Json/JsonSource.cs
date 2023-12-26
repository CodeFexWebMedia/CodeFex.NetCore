using CodeFex.NetCore.Json.Dynamic;
using System;
using System.IO;
using System.Text.Json;
#if NET
using System.Threading.Tasks;
#endif

namespace CodeFex.NetCore.Json
{
        public class JsonSource
    {
        private static ReadOnlySpan<byte> Utf8Bom => new byte[] { 0xEF, 0xBB, 0xBF };

        public static JsonData ReadFromFile(string path, JsonSerializerOptions jsonSerializerOptions)
        {
            if (!File.Exists(path)) return null;

            return JsonSerializer.Deserialize<JsonData>(TrimUtf8Bom(File.ReadAllBytes(path)), jsonSerializerOptions);
        }

#if NET
        public static async Task<JsonData> ReadFromFileAsync(string path, JsonSerializerOptions jsonSerializerOptions)
        {
            if (!File.Exists(path)) return null;

            return JsonSerializer.Deserialize<JsonData>(TrimUtf8Bom(await File.ReadAllBytesAsync(path).ConfigureAwait(false)), jsonSerializerOptions);
        }
#endif

        public static ReadOnlySpan<byte> TrimUtf8Bom(byte[] bytes)
        {
            // UTF8 byte order mark is: 0xEF,0xBB,0xBF
            // According to the Unicode standard, the BOM for UTF-8 files is not recommended:

            ReadOnlySpan<byte> result = bytes;

            // Read past the UTF-8 BOM bytes if a BOM exists.
            return result.StartsWith(Utf8Bom) ? result.Slice(Utf8Bom.Length) : result;
        }
    }
}
