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

    internal interface IOwinHost
    {
        /// <summary>
        /// The host environment, the parent dictionary for all requests
        /// </summary>
        Env Environment { get; }

        /// <summary>
        /// Used to register the service with the host.
        /// The host will call back into the service, allowing the service to configure the host.
        /// </summary>
        /// <param name="service">the service to add to the host</param>
        void AddHostService(IOwinHostService service);

        /// <summary>
        /// The method to run a configured host.
        /// </summary>
        /// <returns>an IDisposable so that the host can be torn down after use</returns>
        IDisposable Run();

        /// <summary>
        /// The application pipeline to use.
        /// Set this with the Pipeline to use, and it will be setup using the host environment.
        /// </summary>
        /// <param name="pipeline"></param>
        void SetApp(Pipeline pipeline);

        /// <summary>
        /// The application function to use.
        /// Set this with the AppFunc to use if application setup is not required.
        /// </summary>
        /// <param name="appFunc"></param>
        void SetApp(AppFunc appFunc);

        /// <summary>
        /// Sets the server to use with this host.
        /// The server is responsible for request management (and networking if applicable)
        /// </summary>
        /// <param name="server">the server to use in this instance</param>
        void SetServer(IOwinServer server);
    }
}