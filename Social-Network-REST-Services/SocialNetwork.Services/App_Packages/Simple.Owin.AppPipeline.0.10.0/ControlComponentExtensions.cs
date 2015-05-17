using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

    internal static class ControlComponentExtensions
    {
        public static void IsGet(this ControlComponent component, string pathRegex, AppFunc appFunc, SetupAction setup = null) {
            component.When(BuildGetFilter(pathRegex), appFunc, setup);
        }

        public static void IsGet(this ControlComponent component, string pathRegex, MiddlewareFunc middlewareFunc, SetupAction setup = null) {
            component.When(BuildGetFilter(pathRegex), middlewareFunc, setup);
        }

        public static void IsGet(this ControlComponent component, string pathRegex, IPipeline pipeline) {
            component.When(BuildGetFilter(pathRegex), pipeline);
        }

        public static void IsGet(this ControlComponent component, string pathRegex, IPipelineComponent handler) {
            component.When(BuildGetFilter(pathRegex), handler);
        }

        private static Func<Env, bool> BuildGetFilter(string pathRegex) {
            var pathMatcher = new Regex(pathRegex);
            return env => {
                       var context = OwinContext.Get(env);
                       if (context.Request.Method != "GET") {
                           return false;
                       }
                       return pathMatcher.IsMatch(context.Request.Path);
                   };
        }
    }
}