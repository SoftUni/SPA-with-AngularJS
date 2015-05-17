using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Simple.Owin.AppPipeline;
using Simple.Owin.Hosting;
using Simple.Owin.Hosting.Trace;

namespace Simple.Owin.Testing
{
    using AppFunc = Func< //
        IDictionary<string, object>, // owin request environment
        Task // completion signal
        >;
    using MiddlewareFunc = Func< //
        IDictionary<string, object>, // owin request environment
        Func<IDictionary<string, object>, Task>, // next AppFunc in pipeline
        Task // completion signal
        >;

    internal class TestHostAndServer : IOwinServer
    {
        private readonly OwinHost _host;
        private readonly StringOutput _traceOutput = new StringOutput();
        private Func<IDictionary<string, object>, Task> _appFunc;

        public TestHostAndServer(AppFunc app, IEnumerable<IOwinHostService> hostServices = null) {
            _host = BuildHost(hostServices);
            _host.SetApp(app);
        }

        public TestHostAndServer(MiddlewareFunc middleware, AppFunc next = null, IEnumerable<IOwinHostService> hostServices = null) {
            _host = BuildHost(hostServices);
            _host.SetApp(environment => middleware(environment, next ?? Pipeline.ReturnDone));
        }

        public TestHostAndServer(Pipeline pipeline, IEnumerable<IOwinHostService> hostServices = null) {
            _host = BuildHost(hostServices);
            _host.SetApp(pipeline);
        }

        public IDictionary<string, object> HostEnvironment {
            get { return _host.Environment; }
        }

        public string TraceOutputValue {
            get { return _traceOutput.Value; }
        }

        public IContext Process(TestRequest request) {
            var requestEnvironment = OwinFactory.CreateScopedEnvironment(_host.Environment);

            var context = OwinContext.Get(requestEnvironment);
            context.Request.PathBase = string.Empty;
            context.Request.Method = request.RequestLine.Method;
            context.Request.FullUri = request.Url;
            context.Request.Protocol = request.RequestLine.HttpVersion;
            context.Request.Headers.MergeIn(request.Headers);
            context.Request.Body = request.Body != null ? new MemoryStream(request.Body, false) : Stream.Null;

            context.Response.Body = new MemoryStream();

            _appFunc(requestEnvironment)
                .Wait();

            return context;
        }

        public IContext ProcessGet(string url) {
            return Process(TestRequest.Get(url));
        }

        private OwinHost BuildHost(IEnumerable<IOwinHostService> hostServices = null) {
            var host = new OwinHost();
            host.AddHostService(_traceOutput);
            if (hostServices != null) {
                foreach (var hostService in hostServices) {
                    host.AddHostService(hostService);
                }
            }
            host.SetServer(this);
            return host;
        }

        void IOwinServer.Configure(OwinHostContext host) {
            host.Environment.Add(OwinKeys.Owin.Version, "1.0");
        }

        void IOwinServer.SetAppFunc(Func<IDictionary<string, object>, Task> appFunc) {
            _appFunc = appFunc;
        }

        IDisposable IOwinServer.Start() {
            throw new Exception("Not required for testing, each call to Process() builds a new scope.");
        }
    }
}