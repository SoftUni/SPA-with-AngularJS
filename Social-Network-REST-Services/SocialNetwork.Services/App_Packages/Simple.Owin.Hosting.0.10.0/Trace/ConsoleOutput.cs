using System;

namespace Simple.Owin.Hosting.Trace
{
    internal class ConsoleOutput : IOwinHostService
    {
        public void Configure(OwinHostContext context) {
            context.AddTraceOutput(Console.Out);
        }
    }
}