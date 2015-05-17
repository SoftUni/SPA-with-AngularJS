using System.Collections.Generic;
using System.IO;
using Simple.Owin.Helpers;
using Simple.Owin.Hosting.Trace;

namespace Simple.Owin.Hosting
{
    internal class OwinHostContext
    {
        private readonly IDictionary<string, object> _environment;

        public OwinHostContext(IDictionary<string, object> environment) {
            _environment = environment;
        }

        public IDictionary<string, object> Environment {
            get { return _environment; }
        }

        public string LocalIpAddress {
            get { return _environment.GetValueOrDefault<string>(OwinKeys.Server.LocalIpAddress); }
            set { _environment.SetValue(OwinKeys.Server.LocalIpAddress, value); }
        }

        public string LocalPort {
            get { return _environment.GetValueOrDefault<string>(OwinKeys.Server.LocalPort); }
            set { _environment.SetValue(OwinKeys.Server.LocalPort, value); }
        }

        public string ServerName {
            get { return _environment.GetValueOrDefault<string>(OwinKeys.Server.Name); }
            set { _environment.SetValue(OwinKeys.Server.Name, value); }
        }

        public TextWriter TraceOutput {
            get { return _environment.GetValueOrDefault<TextWriter>(OwinKeys.Host.TraceOutput) ?? new NullTextWriter(); }
        }

        public string Version {
            get { return _environment.GetValueOrDefault<string>(OwinKeys.Owin.Version); }
            set { _environment.SetValue(OwinKeys.Owin.Version, value); }
        }

        public void AddTraceOutput(TextWriter writer) {
            var output = _environment.GetValueOrDefault<TextWriter>(OwinKeys.Host.TraceOutput);
            if (output == null) {
                _environment.SetValue(OwinKeys.Host.TraceOutput, writer);
                return;
            }
            var multiWriter = output as MultiTextWriter;
            if (multiWriter == null) {
                multiWriter = new MultiTextWriter();
                multiWriter.Add(output);
                multiWriter.Add(writer);
                _environment.SetValue(OwinKeys.Host.TraceOutput, multiWriter);
                return;
            }
            multiWriter.Add(writer);
        }
    }
}