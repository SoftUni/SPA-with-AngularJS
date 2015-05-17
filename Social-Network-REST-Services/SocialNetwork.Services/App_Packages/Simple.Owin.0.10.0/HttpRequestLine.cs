using System;

namespace Simple.Owin
{
    internal class HttpRequestLine
    {
        public HttpRequestLine(string method = null, string requestUri = null, string httpVersion = null) {
            Method = method ?? string.Empty;
            Uri = requestUri ?? string.Empty;
            HttpVersion = httpVersion ?? string.Empty;
            IsValid = (method != null) && (requestUri != null) && (httpVersion != null);
        }

        public string HttpVersion { get; private set; }

        public bool IsValid { get; private set; }

        public string Method { get; private set; }

        public string Uri { get; private set; }

        public static HttpRequestLine Invalid {
            get { return new HttpRequestLine { IsValid = false }; }
        }

        public static HttpRequestLine Parse(string source) {
            string[] parts = source.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            switch (parts.Length) {
                case 0:
                    return Invalid;
                case 1:
                    return new HttpRequestLine(parts[0]);
                case 2:
                    return new HttpRequestLine(parts[0], parts[1]);
                case 3:
                    return new HttpRequestLine(parts[0], parts[1], parts[2]);
                default:
                    return new HttpRequestLine(parts[0], parts[1], parts[2]) { IsValid = false };
            }
        }
    }
}