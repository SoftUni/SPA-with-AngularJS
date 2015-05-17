using System.Collections.Generic;

namespace Simple.Owin
{
    internal class HttpHeaderLine
    {
        private readonly string _name;
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>();
        private readonly string _value;

        public HttpHeaderLine(string httpHeaderLine) {
            var parts = httpHeaderLine.Split(new[] { ':' }, 2);
            _name = parts[0].Trim();
            var parts1 = parts[1].Split(';');
            _value = parts1[0].Trim();
            for (int i = 1; i < parts1.Length; i++) {
                var prop = parts1[i].Split(new[] { '=' }, 2);
                _properties.Add(prop[0], prop[1]);
            }
        }

        public string this[string propertyName] {
            get {
                string value;
                return _properties.TryGetValue(propertyName, out value) ? value : null;
            }
        }

        public string Name {
            get { return _name; }
        }

        public string Value {
            get { return _value; }
        }
    }
}