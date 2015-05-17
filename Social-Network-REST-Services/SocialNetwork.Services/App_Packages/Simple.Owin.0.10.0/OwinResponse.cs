using System;
using System.Collections.Generic;
using System.IO;
using Simple.Owin.Helpers;

namespace Simple.Owin
{
    internal class OwinResponse : IResponse
    {
        private readonly IDictionary<string, object> _environment;
        private readonly OwinResponseHeaders _headers;

        public OwinResponse(IDictionary<string, object> environment) {
            if (environment == null) {
                throw new ArgumentNullException("environment");
            }
            _environment = environment;
            var headers = _environment.GetValueOrCreate(OwinKeys.Response.Headers, OwinFactory.CreateHeaders);
            _headers = new OwinResponseHeaders(headers);
        }

        public Stream Body {
            get { return _environment.GetValue<Stream>(OwinKeys.Response.Body); }
            set { _environment.SetValue(OwinKeys.Response.Body, value); }
        }

        public OwinResponseHeaders Headers {
            get { return _headers; }
        }

        public string Protocol {
            get { return _environment.GetValueOrDefault<string>(OwinKeys.Response.Protocol); }
            set { _environment.SetValue(OwinKeys.Response.Protocol, value); }
        }

        public Status Status {
            get {
                //pull
                var status = _environment.GetValueOrDefault<Status>(OwinKeys.Simple.Status);
                if (status != null) {
                    return status;
                }
                //build
                var code = _environment.GetValueOrDefault(OwinKeys.Response.StatusCode, 0);
                if (code != 0) {
                    return new Status(code, _environment.GetValueOrDefault(OwinKeys.Response.ReasonPhrase, string.Empty));
                }
                //default
                return Status.Is.OK;
            }
            set {
                if (value == null) {
                    _environment.Remove(OwinKeys.Response.StatusCode);
                    _environment.Remove(OwinKeys.Response.ReasonPhrase);
                }
                else {
                    _environment.SetValue(OwinKeys.Response.StatusCode, value.Code);
                    _environment.SetValue(OwinKeys.Response.ReasonPhrase, value.Description);
                    if (value.LocationHeader != null) {
                        _headers.Location = value.LocationHeader;
                    }
                }
            }
        }

        public void OnSendingHeaders(Action<object> callback, object state) {
            var serverOnSendingHeaders = _environment.GetValueOrDefault<Action<Action<object>, object>>(OwinKeys.Server.OnSendingHeaders);
            if (serverOnSendingHeaders == null) {
                throw new NotSupportedException("The server does not support 'OnSendingHeaders'");
            }
            serverOnSendingHeaders(callback, state);
        }

        public void RemoveCookie(string cookieName) {
            _headers.Add(HttpHeaderKeys.SetCookie, string.Format("{0}=; Expires=Thu, 01 Jan 1970 00:00:00 GMT", cookieName));
        }

        public void SetLastModified(DateTime when) {
            _headers.LastModified = when.ToUniversalTime()
                                        .ToString("R");
        }

        public void SetLastModified(DateTimeOffset when) {
            _headers.LastModified = when.ToString("r");
        }

        IResponseHeaders IResponse.Headers {
            get { return _headers; }
        }
    }
}