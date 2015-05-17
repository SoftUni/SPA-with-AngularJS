using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Simple.Owin.Helpers;

namespace Simple.Owin
{
    internal class OwinContext : IContext, IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _environment;
        private readonly OwinRequest _request;
        private readonly OwinResponse _response;

        static OwinContext() {
            var assemblyName = Assembly.GetExecutingAssembly()
                                       .GetName();
            ContextKey = string.Format("{0}:{1}:{2}", OwinKeys.Simple.Context, assemblyName.Name, assemblyName.Version.ToString(3));
        }

        public OwinContext(IDictionary<string, object> environment = null) {
            _environment = environment ?? OwinFactory.CreateEnvironment();
            if (!_environment.ContainsKey(OwinKeys.Owin.CallCancelled)) {
                _environment.Add(OwinKeys.Owin.CallCancelled, new CancellationToken());
            }
            _request = new OwinRequest(_environment);
            _response = new OwinResponse(_environment);
        }

        public CancellationToken CancellationToken {
            get { return _environment.GetValue<CancellationToken>(OwinKeys.Owin.CallCancelled); }
        }

        public IDictionary<string, object> Environment {
            get { return _environment; }
        }

        public string OwinVersion {
            get { return _environment.GetValue<string>(OwinKeys.Owin.Version); }
            set { _environment.SetValue(OwinKeys.Owin.Version, value); }
        }

        public OwinRequest Request {
            get { return _request; }
        }

        public OwinResponse Response {
            get { return _response; }
        }

        public TextWriter TraceOutput {
            get { return _environment.GetValueOrDefault<TextWriter>(OwinKeys.Host.TraceOutput); }
            set { _environment.SetValue(OwinKeys.Host.TraceOutput, value); }
        }

        int ICollection<KeyValuePair<string, object>>.Count {
            get { return _environment.Count; }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly {
            get { return _environment.IsReadOnly; }
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) {
            _environment.Add(item);
        }

        void ICollection<KeyValuePair<string, object>>.Clear() {
            _environment.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) {
            return _environment.Contains(item);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
            _environment.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) {
            return _environment.Remove(item);
        }

        IRequest IContext.Request {
            get { return _request; }
        }

        IResponse IContext.Response {
            get { return _response; }
        }

        object IDictionary<string, object>.this[string key] {
            get { return _environment[key]; }
            set { _environment[key] = value; }
        }

        ICollection<string> IDictionary<string, object>.Keys {
            get { return _environment.Keys; }
        }

        ICollection<object> IDictionary<string, object>.Values {
            get { return _environment.Values; }
        }

        void IDictionary<string, object>.Add(string key, object value) {
            _environment.Add(key, value);
        }

        bool IDictionary<string, object>.ContainsKey(string key) {
            return _environment.ContainsKey(key);
        }

        bool IDictionary<string, object>.Remove(string key) {
            return _environment.Remove(key);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value) {
            return _environment.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _environment.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() {
            return _environment.GetEnumerator();
        }

        private static readonly string ContextKey;

        public static OwinContext Get(IDictionary<string, object> environment = null) {
            environment = environment ?? OwinFactory.CreateEnvironment();
            return environment.GetValueOrCreate(ContextKey, () => new OwinContext(environment));
        }
    }
}