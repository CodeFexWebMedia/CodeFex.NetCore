using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;

namespace CodeFex.NetCore.Http.Client.Factories
{
    public class HttpClientFactory
    {
        public static readonly HttpClient DefaultHttpClient = new HttpClient(CreateHttpMessageHandler(false), true);

        public static HttpClient CreateHttpClient(bool allowSelfSignedCerts = false, ICredentials credentials = null)
        {
#if NETFRAMEWORK || NETSTANDARD
            var handler = CreateHttpClientHandler(false);

            handler.Credentials = credentials;
            handler.PreAuthenticate = (credentials != null);
            handler.UseDefaultCredentials = (credentials != null);

            return new HttpClient(handler, true);
#endif

#if NET
            var handler = CreateSocketsHttpHandler(allowSelfSignedCerts);

            handler.Credentials = credentials;
            handler.PreAuthenticate = (handler.Credentials != null);

            var result = new HttpClient(handler, true);
            result.DefaultRequestHeaders.ExpectContinue = false;

            return result;
#endif
        }

        public static HttpMessageHandler CreateHttpMessageHandler(bool allowSelfSignedCerts = false)
        {
#if NETFRAMEWORK || NETSTANDARD
            return CreateHttpClientHandler(allowSelfSignedCerts);
#endif

#if NET
            return CreateSocketsHttpHandler(allowSelfSignedCerts);
#endif
        }


#if NETFRAMEWORK || NETSTANDARD
        public static HttpClientHandler CreateHttpClientHandler(bool allowSelfSignedCerts = false)
        {
            var result = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                MaxConnectionsPerServer = int.MaxValue,
                PreAuthenticate = false,
                Proxy = null,
                UseCookies = false,
                UseProxy = false
            };

            if (allowSelfSignedCerts)
            {
                result.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            }

            return result;
        }
#endif

#if NET
        public static SocketsHttpHandler CreateSocketsHttpHandler(bool allowSelfSignedCerts = false)
        {
            try
            {
                // https://learn.microsoft.com/en-us/dotnet/api/system.net.http.socketshttphandler?view=net-7.0
                var result = new SocketsHttpHandler()
                {
#if NET6_0
                    // Gets or sets the propagator to use when propagating the distributed trace and context. Use null to disable propagation.
                    ActivityHeadersPropagator = null,
#endif

                    // Gets or sets a value that indicates whether the handler should follow redirection responses.
                    AllowAutoRedirect = false,

                    // Gets or sets the type of decompression method used by the handler for automatic decompression of the HTTP content response.
                    AutomaticDecompression = DecompressionMethods.All,

                    // Gets or sets a custom callback used to open new connections.
                    ConnectCallback = null,

                    // Gets or sets the timespan to wait before the connection establishing times out.
                    ConnectTimeout = TimeSpan.FromSeconds(20),

                    // Gets or sets the managed cookie container object.
                    // CookieContainer

                    // Gets or sets authentication information used by this handler.
                    // Credentials

                    // When the default(system) proxy is used, gets or sets the credentials used to submit to the default proxy server for authentication.
                    // DefaultProxyCredentials

                    // Gets or sets a value that indicates whether additional HTTP / 2 connections can be established to the same server when the maximum number of concurrent streams is reached on all existing connections.
                    // EnableMultipleHttp2Connections

                    // Gets or sets the time -out value for server HTTP 100 Continue response.
                    // Expect100ContinueTimeout

                    // Defines the initial HTTP2 stream receive window size for all connections opened by the this SocketsHttpHandler.
                    // InitialHttp2StreamWindowSize

                    // Gets a value that indicates whether the handler is supported on the current platform.
                    // IsSupported

                    // Gets or sets the keep alive ping delay.
                    // KeepAlivePingDelay

                    // Gets or sets the keep alive ping behaviour.
                    // KeepAlivePingPolicy

                    // Gets or sets the keep alive ping timeout.
                    // KeepAlivePingTimeout

                    // Gets or sets the maximum number of allowed HTTP redirects.
                    // MaxAutomaticRedirections

                    // Gets or sets the maximum number of simultaneous TCP connections allowed to a single server.
                    MaxConnectionsPerServer = int.MaxValue,

                    // Gets or sets the maximum amount of data that can be drained from responses in bytes.
                    // MaxResponseDrainSize

                    // Gets or sets the maximum length, in kilobytes(1024 bytes), of the response headers.
                    // MaxResponseHeadersLength

                    // Gets or sets a custom callback that provides access to the plaintext HTTP protocol stream.
                    // PlaintextStreamFilter

                    // Gets or sets how long a connection can be idle in the pool to be considered reusable.
                    // PooledConnectionIdleTimeout

                    // Gets or sets how long a connection can be in the pool to be considered reusable.
                    // This lifetime is useful in order to allow connections to be reestablished periodically so as to better reflect DNS or other network changes.
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),

                    // Gets or sets a value that indicates whether the handler sends an Authorization header with the request.
                    PreAuthenticate = false,

                    // Gets a writable dictionary(that is, a map) of custom properties for the HttpClient requests.The dictionary is initialized empty; you can insert and query key-value pairs for your custom handlers and special processing.
                    // Properties

                    // Gets or sets the custom proxy when the UseProxy property is true.
                    Proxy = null,

                    // Gets or sets a callback that selects the Encoding to encode request header values.
                    // RequestHeaderEncodingSelector

                    // Gets or sets the timespan to wait for data to be drained from responses.
                    // ResponseDrainTimeout

                    // Gets or sets a callback that selects the Encoding to decode response header values.
                    // ResponseHeaderEncodingSelector

                    // Gets or sets the set of options used for client TLS authentication.
                    // SslOptions = allowSelfSignedCerts ? new SslClientAuthenticationOptions { RemoteCertificateValidationCallback = delegate { return true; } } : null,

                    // Gets or sets a value that indicates whether the handler should use cookies.
                    UseCookies = false,

                    // Gets or sets a value that indicates whether the handler should use a proxy.
                    UseProxy = false
                };

                if (allowSelfSignedCerts)
                {
                    result.SslOptions = new SslClientAuthenticationOptions { RemoteCertificateValidationCallback = delegate { return true; } };
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }
#endif
    }
}