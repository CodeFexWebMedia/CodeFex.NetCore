using System.Text.Json;

namespace CodeFex.NetCore.Http.Client.Json.Problem
{
    public class HttpJsonProblemContent : HttpJsonContent
    {
        public HttpJsonProblemContent(JsonProblem value, JsonSerializerOptions jsonSerializerOptions) : base(value, jsonSerializerOptions)
        {
            Headers.ContentType = ContentTypes.MediaTypeJsonProblem;
        }
    }
}
