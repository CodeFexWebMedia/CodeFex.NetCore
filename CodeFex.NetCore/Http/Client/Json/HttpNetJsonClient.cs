using CodeFex.NetCore.Data.Collections.Generic;
using CodeFex.NetCore.Json;
using CodeFex.NetCore.Json.Serialization;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CodeFex.NetCore.Http.Client.Json
{
    public class HttpNetJsonClient
    {
        public HttpClient HttpClient { get; set; }
        public JsonSerializerOptions JsonSerializerOptions { get; set; }
        public GenericCollection<string, string> SessionHeaders { get; set; }
        public HashSet<string> WatchHeaders { get; set; }
        public bool CaptureHttpHeaders { get; set; }
        public StreamWriter StreamWriter { get; set; }

        public HttpNetJsonClient(JsonSerializerOptions jsonSerializerOptions = null)
        {
            JsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions(JsonSerializationDefaults.JsonApiDefaults);
        }

        public HttpNetJsonClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions = null) : this(jsonSerializerOptions)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<Stream> ReadAsStreamAync(HttpContent httpContent)
        {
            if (httpContent == null) return null;

#if NETFRAMEWORK || NETSTANDARD
            var result = new MemoryStream();
            await httpContent.CopyToAsync(result).ConfigureAwait(false);
            return result;
#endif

#if NET
            return await httpContent.ReadAsStreamAsync().ConfigureAwait(false);
#endif
        }

        protected async Task<HttpJsonResult<S, E>> Http<S, E>(HttpMethod httpMethod, Uri uri, HttpJsonContent payload) where S : class where E : class
        {
            using (var request = new HttpRequestMessage(httpMethod, uri))
            {
                request.Content = payload;

                if (SessionHeaders != null)
                {
                    foreach (var header in SessionHeaders)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                request.Headers.TryAddWithoutValidation(HeaderNames.Accept, ContentTypes.AppJson);

                if (StreamWriter != null)
                {
                    await StreamWriter.WriteLineAsync(string.Empty).ConfigureAwait(false);
                    await StreamWriter.WriteLineAsync(string.Concat("-> ", request.Method.ToString(), " ", uri.ToString())).ConfigureAwait(false);

                    if (WatchHeaders != null)
                    {
                        foreach (var header in request.Headers)
                        {
                            if (WatchHeaders.Contains(header.Key))
                            {
                                await StreamWriter.WriteLineAsync(string.Concat(header.Key, "=", header.Value.FirstOrDefault())).ConfigureAwait(false);
                            }
                        }
                    }

                    if (payload != null)
                    {
                        await StreamWriter.WriteAsync(JsonText.PrettyPrint(await ReadAsStreamAync(payload).ConfigureAwait(false))).ConfigureAwait(false);
                    }
                }

                var started = DateTime.Now.Ticks;
                using (var response = await HttpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var result = await new HttpJsonResult<S, E>().From(response, started, JsonSerializerOptions, CaptureHttpHeaders).ConfigureAwait(false);

                    if (StreamWriter != null)
                    {
                        await StreamWriter.WriteLineAsync(string.Concat("<> ", result.Duration, " ms")).ConfigureAwait(false);
                        await StreamWriter.WriteLineAsync(string.Concat("<- ", ((int)response.StatusCode).ToString(), " ", response.ReasonPhrase)).ConfigureAwait(false);

                        if (WatchHeaders != null)
                        {
                            foreach (var header in response.Headers)
                            {
                                if (WatchHeaders.Contains(header.Key))
                                {
                                    await StreamWriter.WriteLineAsync(string.Concat(header.Key, "=", header.Value.FirstOrDefault())).ConfigureAwait(false);
                                }
                            }
                        }

                        if (result.IsOk)
                        {
                            await StreamWriter.WriteAsync(JsonText.PrettyPrint(await ReadAsStreamAync(response.Content).ConfigureAwait(false))).ConfigureAwait(false);
                        }
                        else
                        {
                            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            if (responseBody != null)
                            {
                                await StreamWriter.WriteLineAsync(responseBody).ConfigureAwait(false);
                            }
                        }
                    }

                    return result;
                }
            }
        }

        public async Task<HttpJsonResult<S, E>> Head<S, E>(Uri uri) where S : class where E : class
        {
            return await Http<S, E>(HttpMethod.Head, uri, null).ConfigureAwait(false);
        }

        public async Task<HttpJsonResult<S, E>> Get<S, E>(Uri uri) where S : class where E : class
        {
            return await Http<S, E>(HttpMethod.Get, uri, null).ConfigureAwait(false);
        }

        public async Task<HttpJsonResult<S, E>> Post<S, E>(Uri uri, HttpJsonContent payload) where S : class where E : class
        {
            return await Http<S, E>(HttpMethod.Post, uri, payload).ConfigureAwait(false);
        }

        public async Task<HttpJsonResult<S, E>> Put<S, E>(Uri uri, HttpJsonContent payload) where S : class where E : class
        {
            return await Http<S, E>(HttpMethod.Put, uri, payload).ConfigureAwait(false);
        }

        public async Task<HttpJsonResult<S, E>> Delete<S, E>(Uri uri, HttpJsonContent payload) where S : class where E : class
        {
            return await Http<S, E>(HttpMethod.Delete, uri, payload).ConfigureAwait(false);
        }
    }
}
