using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Simple.Owin.AppPipeline;

namespace Simple.Owin.Hosting
{
    using Env = IDictionary<string, object>;
    using AppFunc = Func< //
        IDictionary<string, object>, // owin request environment
        Task // completion signal
        >;

    internal class OwinHost : IOwinHost
    {
        private readonly OwinHostContext _hostContext;
        private IOwinServer _server;
        private OwinHostState _state;

        public OwinHost() {
            _hostContext = new OwinHostContext(OwinFactory.CreateEnvironment());
            _state = OwinHostState.ConfigureHost;
        }

        public Env Environment {
            get { return _hostContext.Environment; }
        }

        public void AddHostService(IOwinHostService service) {
            switch (_state) {
                case OwinHostState.ConfigureHost:
                    service.Configure(_hostContext);
                    return;
                case OwinHostState.ConfigureApp:
                case OwinHostState.Runnable:
                    throw new Exception("Host Services must be specified before setting the Server.");
                default:
                    throw new ArgumentOutOfRangeException("Unknown Host State: " + _state);
            }
        }

        public IDisposable Run() {
            switch (_state) {
                case OwinHostState.ConfigureHost:
                    throw new Exception("The Server has not been set.");
                case OwinHostState.ConfigureApp:
                    throw new Exception("The AppFunc has not been set.");
                case OwinHostState.Runnable:
                    return _server.Start();
                default:
                    throw new ArgumentOutOfRangeException("Unknown Host State: " + _state);
            }
        }

        public void SetApp(Pipeline pipeline) {
            switch (_state) {
                case OwinHostState.ConfigureHost:
                    throw new Exception("The Server must be specified before setting the AppFunc.");
                case OwinHostState.ConfigureApp:
                    pipeline.Setup(_hostContext.Environment);
                    var app = pipeline.Build();
                    SetApp(app);
                    return;
                case OwinHostState.Runnable:
                    throw new Exception("The AppFunc may only be set once.");
                default:
                    throw new ArgumentOutOfRangeException("Unknown Host State: " + _state);
            }
        }

        public void SetApp(AppFunc appFunc) {
            switch (_state) {
                case OwinHostState.ConfigureHost:
                    throw new Exception("The Server must be specified before setting the AppFunc.");
                case OwinHostState.ConfigureApp:
                    _server.SetAppFunc(appFunc);
                    _state = OwinHostState.Runnable;
                    return;
                case OwinHostState.Runnable:
                    throw new Exception("The AppFunc may only be set once.");
                default:
                    throw new ArgumentOutOfRangeException("Unknown Host State: " + _state);
            }
        }

        public void SetServer(IOwinServer server) {
            switch (_state) {
                case OwinHostState.ConfigureHost:
                    _server = server;
                    _server.Configure(new OwinHostContext(_hostContext.Environment));
                    _state = OwinHostState.ConfigureApp;
                    return;
                case OwinHostState.ConfigureApp:
                case OwinHostState.Runnable:
                    throw new Exception("The Server may only be set once.");
                default:
                    throw new ArgumentOutOfRangeException("Unknown Host State: " + _state);
            }
        }
    }
}