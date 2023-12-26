using System.Net.Http;
using System.Text.Json;

namespace CodeFex.NetCore.Http.Client.Json.Jose
{
    public class HttpJsonJoseContent : ByteArrayContent
    {
        public HttpJsonJoseContent(object value, JsonSerializerOptions jsonSerializerOptions) : base(JsonSerializer.SerializeToUtf8Bytes(value, jsonSerializerOptions))
        {
            Headers.ContentType = ContentTypes.MediaTypeJson;
        }
    }
}
