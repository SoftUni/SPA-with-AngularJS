using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Simple.Owin
{
    internal static class OwinFactory
    {
        private static Func<IDictionary<string, object>> _createEnvironment =
            () => new ConcurrentDictionary<string, object>(StringComparer.Ordinal);

        private static Func<IDictionary<string, string[]>> _createHeaders =
            () => new ConcurrentDictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        private static Func<IDictionary<string, object>, IDictionary<string, object>> _createScopedEnvironment =
            parent => new ConcurrentDictionary<string, object>(parent, StringComparer.Ordinal);

        public static Func<IDictionary<string, object>> CreateEnvironment {
            get { return _createEnvironment; }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                _createEnvironment = value;
            }
        }

        public static Func<IDictionary<string, string[]>> CreateHeaders {
            get { return _createHeaders; }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                _createHeaders = value;
            }
        }

        public static Func<IDictionary<string, object>, IDictionary<string, object>> CreateScopedEnvironment {
            get { return _createScopedEnvironment; }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                _createScopedEnvironment = value;
            }
        }
    }
}