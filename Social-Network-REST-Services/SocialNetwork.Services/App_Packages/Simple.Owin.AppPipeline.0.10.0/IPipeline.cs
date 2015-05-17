using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    internal interface IPipeline
    {
        AppFunc Build();

        void Setup(Env hostEnvironment);

        void Use(AppFunc app, SetupAction setup = null);

        IPipeline Use(MiddlewareFunc middleware, SetupAction setup = null);

        IPipeline Use(IPipelineComponent component);
    }
}