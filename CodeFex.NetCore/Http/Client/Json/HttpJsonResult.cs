using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CodeFex.NetCore.Http.Client.Json
{
    public interface IHttpJsonResponse : IHttpResult
    {
        object Response { get; }
    }

    public class HttpJsonResult<S, E> : HttpResult, IHttpJsonResponse where S : class where E : class
    {
        public S Result { get; protected set; }
        public E Error { get; protected set; }

        public object Response
        {
            get
            {
                return (object)Result ?? (object)Error;
            }
        }

        public HttpJsonResult()
        {
        }

        public async Task<HttpJsonResult<S, E>> From(HttpResponseMessage httpResponseMessage, long started, JsonSerializerOptions jsonSerializerOptions, bool captureHttpHeaders = false)
        {
            try
            {
                await From(httpResponseMessage, started).ConfigureAwait(false);

                if (captureHttpHeaders)
                {
                    HttpHeaders = new HttpGenericHeaders();

                    foreach (KeyValuePair<string, IEnumerable<string>> header in httpResponseMessage.Headers)
                    {
                        HttpHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // 

                var contentType = httpResponseMessage.Content.Headers.ContentType;
                if (contentType != null)
                {
                    if (ContentTypes.MediaTypeJsonVariants.IsVariantOf(contentType.MediaType))
                    {
                        if (IsOk)
                        {
                            Result = JsonSerializer.Deserialize<S>(new ReadOnlySpan<byte>(Content), jsonSerializerOptions);
                        }
                        else
                        {
                            Error = JsonSerializer.Deserialize<E>(new ReadOnlySpan<byte>(Content), jsonSerializerOptions);
                        }
                    }
                }
                else
                {
                    // request HEAD dooes ot contain content
                    // throw new Exception(string.Concat("HttpResponseMessage ContentType is not ", ContentTypes.MediaTypeJson.ToString()));
                }
            }
            catch (Exception ex)
            {
                Exception = ex;
            }

            return this;
        }
    }
}
