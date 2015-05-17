using System;
using System.Globalization;
using System.Text;

namespace Simple.Owin.Testing
{
    internal class TestRequest
    {
        private readonly HttpHeaders _headers = new HttpHeaders();
        private readonly HttpRequestLine _requestLine;
        private byte[] _body;
        private Uri _url;

        private TestRequest(HttpRequestLine requestLine) {
            _requestLine = requestLine;
        }

        internal byte[] Body {
            get { return _body; }
        }

        internal HttpHeaders Headers {
            get { return _headers; }
        }

        internal HttpRequestLine RequestLine {
            get { return _requestLine; }
        }

        internal Uri Url {
            get { return _url ?? (_url = BuildUrl()); }
        }

        public TestRequest WithContent(string value, Encoding encoding = null) {
            encoding = encoding ?? Encoding.UTF8;
            _body = encoding.GetBytes(value);
            _headers.Add(HttpHeaderKeys.ContentLength, _body.Length.ToString(CultureInfo.InvariantCulture));
            return this;
        }

        public TestRequest WithContentType(string value) {
            _headers.Add(HttpHeaderKeys.ContentType, value);
            return this;
        }

        public TestRequest WithCookie(string name, string value) {
            _headers.Add(HttpHeaderKeys.Cookie, string.Format("{0}={1}", name, value));
            return this;
        }

        public TestRequest WithFile(string name, string localPath) {
            throw new NotImplementedException();
        }

        public TestRequest WithFormData(string name, string value) {
            throw new NotImplementedException();
        }

        public TestRequest WithHeader(string name, string value) {
            _headers.Add(name, value);
            return this;
        }

        private Uri BuildUrl() {
            if (RequestLine.Uri.StartsWith("http")) {
                return new Uri(RequestLine.Uri, UriKind.Absolute);
            }
            if (RequestLine.Uri.StartsWith("/")) {
                string host = _headers.GetValue(HttpHeaderKeys.Host) ?? "localhost";
                return new Uri("http://" + host + RequestLine.Uri, UriKind.Absolute);
            }
            throw new Exception("invalid test url format.");
        }

        public static TestRequest Delete(string url) {
            return new TestRequest(new HttpRequestLine("DELETE", url, "HTTP/1.1"));
        }

        public static TestRequest Get(string url) {
            return new TestRequest(new HttpRequestLine("GET", url, "HTTP/1.1"));
        }

        public static TestRequest Post(string url) {
            return new TestRequest(new HttpRequestLine("POST", url, "HTTP/1.1"));
        }

        public static TestRequest Put(string url) {
            return new TestRequest(new HttpRequestLine("PUT", url, "HTTP/1.1"));
        }
    }
}