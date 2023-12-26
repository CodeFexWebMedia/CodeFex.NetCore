using CodeFex.NetCore.Http.Client.Factories;
using Microsoft.Net.Http.Headers;
using MimeTypes;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CodeFex.NetCore.Http.Client
{
    public interface IHttpResult
    {
        bool IsOk { get; }

        string Url { get; }

        HttpStatusCode ResponseCode { get; }

        string ResponseStatus { get; }

        HttpGenericHeaders HttpHeaders { get; }

        byte[] Content { get; }

        string ContentType { get; }

        string FileExtension { get; }

        long? Duration { get; }

        Exception Exception { get; }
    }

    public class HttpResult : IHttpResult
    {
        public bool IsOk
        {
            get
            {
                return (Exception == null) && ((ResponseCode == HttpStatusCode.OK) || (ResponseCode == HttpStatusCode.Created) || (ResponseCode == HttpStatusCode.NoContent));
            }
        }
        public string Url { get; protected set; }

        public HttpStatusCode ResponseCode { get; protected set; }

        public string ResponseStatus { get; protected set; }

        public HttpGenericHeaders HttpHeaders { get; protected set; }

        public byte[] Content { get; protected set; }

        public string ContentType { get; protected set; }

        public string FileExtension { get; protected set; }

        public long? Duration { get; protected set; }

        public Exception Exception { get; protected set; }

        public HttpResult()
        {
        }

        protected string DetectFileExtension(HttpContentHeaders headers)
        {
            return MimeTypeMap.GetExtension(headers.ContentType.MediaType, false);
        }

        public async Task<HttpResult> From(HttpResponseMessage httpResponseMessage, long started)
        {
            if (httpResponseMessage == null) throw new ArgumentNullException(nameof(httpResponseMessage));

            try
            {
                Url = httpResponseMessage.RequestMessage?.RequestUri?.ToString();

                Duration = (DateTime.Now.Ticks - started) / TimeSpan.TicksPerMillisecond;

                ResponseCode = httpResponseMessage.StatusCode;
                ResponseStatus = httpResponseMessage.ReasonPhrase;

                if (httpResponseMessage.Content?.Headers?.ContentType != null)
                {
                    ContentType = httpResponseMessage.Content.Headers.ContentType.ToString();
                    FileExtension = DetectFileExtension(httpResponseMessage.Content.Headers);
                    Content = await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Exception = ex;
            }

            return this;
        }
    }

    public class HttpNetClient
    {
        public static readonly HttpNetClient Instance = new HttpNetClient();

        // User-Agent: Mozilla/5.0 (<system-information>) <platform> (<platform-details>) <extensions>
        // Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0

        // CodeFex.NetCore/x.x.x.x (Windows; x64; .NET/x.x.x) HttpNetClient/x.x.x.x
        public static readonly string UserAgent;

        public HttpClient HttpClient { get; protected set; }

        static HttpNetClient()
        {
            try
            {
                string platform = null;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    platform = nameof(OSPlatform.Windows);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    platform = nameof(OSPlatform.Linux);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    platform = nameof(OSPlatform.OSX);
                }
#if NET
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                {
                    platform = nameof(OSPlatform.FreeBSD);
                }
#endif
                else
                {
                    platform = "UknownOS";
                }

                var frameworkVersion = RuntimeInformation.FrameworkDescription ?? ".Net Framework";
                if (frameworkVersion != null)
                {
                    var chars = frameworkVersion.ToCharArray();

                    var prevIsSpace = false;
                    var isDigit = false;
                    for (var i = 0; i < chars.Length; i++)
                    {
                        isDigit = char.IsDigit(chars[i]);

                        if (isDigit && prevIsSpace)
                        {
                            chars[i - 1] = '/';
                            frameworkVersion = new string(chars);

                            i = chars.Length;
                            continue; // break
                        }

                        prevIsSpace = chars[i] == ' ';

                    }
                }

                platform = string.Format("({0}; x{1}; {2})", platform, Environment.Is64BitOperatingSystem ? "64" : "32", frameworkVersion);

                var assembly = typeof(HttpNetClient).Assembly;

                UserAgent = string.Format("{0}/{1} {2} {3}/{4}", assembly.GetName().Name, assembly.GetName().Version.ToString(), platform, nameof(HttpNetClient), assembly.GetName().Version.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                UserAgent = string.Concat(nameof(HttpNetClient), "/", "1.0");
            }
        }

        public DomainLimiter DomainLimiter { get; protected set; }

        public HttpNetClient()
        {
        }

        public HttpNetClient(HttpClient httpClient = null)
        {
            HttpClient = httpClient;
        }

        public HttpNetClient(HttpClient httpClient = null, bool useDomainLimiter = false) : this(httpClient)
        {
            if (useDomainLimiter)
            {
                DomainLimiter = new DomainLimiter();
            }
        }

        public async Task<HttpResult> Get(string url, DomainLimiter domainLimiter = null)
        {
            var uri = new Uri(url);

            if (domainLimiter != null)
            {
                await domainLimiter.Acquire(uri).ConfigureAwait(false);
            }

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                if (UserAgent != null)
                {
                    request.Headers.Add(HeaderNames.UserAgent, UserAgent);
                }

                var started = DateTime.Now.Ticks;
                using (var response = await (HttpClient ?? HttpClientFactory.DefaultHttpClient).SendAsync(request).ConfigureAwait(false))
                {
                    return await new HttpResult().From(response, started).ConfigureAwait(false);
                }
            }
        }
    }
}
