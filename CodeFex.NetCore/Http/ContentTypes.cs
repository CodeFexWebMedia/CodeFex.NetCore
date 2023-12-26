using System.Net.Http.Headers;

namespace CodeFex.NetCore.Http
{
    public class ContentTypes
    {
        public const string TextPlain = "text/plain; charset=utf-8";
        public const string AppXml = "application/xml";

        public const string AppJson = "application/json; charset=utf-8";

        public const string AppJsonProblem = "application/problem+json; charset=utf-8";
        public const string AppJsonJose = "application/jose+json; charset=utf-8";

        public static readonly MediaTypeHeaderValue MediaTypeJson = MediaTypeHeaderValue.Parse(AppJson);
        public static readonly MediaTypeHeaderValue MediaTypeJsonProblem = MediaTypeHeaderValue.Parse(AppJsonProblem);
        public static readonly MediaTypeHeaderValue MediaTypeJsonJose = MediaTypeHeaderValue.Parse(AppJsonJose);

        public static readonly MimeTypeVariants MediaTypeJsonVariants = new MimeTypeVariants(AppJson, new[] { "problem", "jose", "geo"});
    }
}