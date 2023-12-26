using System.Net.Http;
using System.Text.Json;

namespace CodeFex.NetCore.Http.Client.Json
{
    public class HttpJsonContent : ByteArrayContent
    {
        public static readonly HttpJsonContent Empty = new HttpJsonContent(new object(), null);

        public HttpJsonContent(object value, JsonSerializerOptions jsonSerializerOptions) : base(JsonSerializer.SerializeToUtf8Bytes(value, jsonSerializerOptions))
        {
            Headers.ContentType = ContentTypes.MediaTypeJson;
        }
    }
}
