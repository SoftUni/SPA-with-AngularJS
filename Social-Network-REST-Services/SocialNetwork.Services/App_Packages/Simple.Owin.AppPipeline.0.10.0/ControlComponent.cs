using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Simple.Owin.Helpers;

namespace Simple.Owin.AppPipeline
{
    using Env = IDictionary<string, object>;
    using AppFunc = Func< //
        IDictionary<string, object>, // owin request environment
        Task // completion signal
        >;
    using MiddlewareFunc = Func< //
        IDictionary<string, object>, // owin request environment
        Func<IDictionary<string, object>, Task>, // next AppFunc in pipeline
        Task // completion signal
        >;
    using SetupAction = Action< //
        IDictionary<string, object> // owin host environment
        >;

    internal class ControlComponent : IPipelineComponent, IFluentApi
    {
        public enum Match
        {
            First,
            All
        }

        private readonly Match _match;
        private readonly List<ControlOption> _options = new List<ControlOption>();
        private AppFunc _next;

        public ControlComponent(Match match) {
            _match = match;
        }

        public void When(Func<Env, bool> shouldHandle, AppFunc appFunc, SetupAction setup = null) {
            When(shouldHandle, new DelegateComponent((env, _) => appFunc(env), setup));
        }

        public void When(Func<Env, bool> shouldHandle, MiddlewareFunc middlewareFunc, SetupAction setup = null) {
            When(shouldHandle, new DelegateComponent(middlewareFunc, setup));
        }

        public void When(Func<Env, bool> shouldHandle, IPipeline pipeline) {
            When(shouldHandle, new NestedPipeline(pipeline));
        }

        public void When(Func<Env, bool> shouldHandle, IPipelineComponent handler) {
            _options.Add(new ControlOption(shouldHandle, handler));
        }

        void IPipelineComponent.Connect(AppFunc next) {
            _next = next;
            if (_match == Match.First) {
                foreach (var option in _options) {
                    option.Handler.Connect(_next);
                }
            }
        }

        Task IPipelineComponent.Execute(Env requestEnvironment) {
            switch (_match) {
                case Match.First:
                    var option = _options.FirstOrDefault(o => o.ShouldDo(requestEnvironment));
                    if (option != null) {
                        return option.Handler.Execute(requestEnvironment);
                    }
                    return _next(requestEnvironment);
                case Match.All:
                    var tasks = _options.Where(o => o.ShouldDo(requestEnvironment))
                                        .Select(o => o.Handler.Execute(requestEnvironment))
                                        .ToArray();
                    if (tasks.Length > 0) {
                        try {
                            Task.WaitAll(tasks,
                                         OwinContext.Get(requestEnvironment)
                                                    .CancellationToken);
                        }
                        catch (Exception ex) {
                            return TaskHelper.Exception(ex);
                        }
                    }
                    return _next(requestEnvironment);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void IPipelineComponent.Setup(Env hostEnvironment) {
            //throw if no options set?
            foreach (var option in _options) {
                option.Handler.Setup(hostEnvironment);
            }
        }

        private class ControlOption
        {
            public ControlOption(Func<Env, bool> shouldDo, IPipelineComponent handler) {
                ShouldDo = shouldDo;
                Handler = handler;
            }

            public IPipelineComponent Handler { get; set; }

            public Func<Env, bool> ShouldDo { get; set; }
        }

        private class NestedPipeline : IPipelineComponent
        {
            private readonly IPipeline _pipeline;
            private AppFunc _app;

            public NestedPipeline(IPipeline pipeline) {
                _pipeline = pipeline;
            }

            public void Connect(AppFunc next) {
                _pipeline.Use((env, _) => next(env));
                _app = _pipeline.Build();
            }

            public Task Execute(Env requestEnvironment) {
                return _app(requestEnvironment);
            }

            public void Setup(Env hostEnvironment) {
                _pipeline.Setup(hostEnvironment);
            }
        }
    }
}