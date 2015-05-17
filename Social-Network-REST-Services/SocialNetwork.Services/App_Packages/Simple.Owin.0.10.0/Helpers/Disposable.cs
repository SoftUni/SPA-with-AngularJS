using System;
using System.Threading;

namespace Simple.Owin.Helpers
{
    internal static class Disposable
    {
        /// <summary>
        ///     Creates a disposable object that invokes the specified action when disposed.
        /// </summary>
        /// <param name="dispose">
        ///     Action to run during the first call to <see cref="M:System.IDisposable.Dispose" />. The action is guaranteed to be
        ///     run at most once.
        /// </param>
        /// <returns>
        ///     The disposable object that runs the given action upon disposal.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="dispose" /> is null.
        /// </exception>
        public static IDisposable Create(Action dispose) {
            return new AnonymousDisposable(dispose);
        }

        private sealed class AnonymousDisposable : IDisposable
        {
            private Action _dispose;

            public AnonymousDisposable(Action dispose) {
                _dispose = dispose;
            }

            /// <summary>
            ///     Calls the disposal action if and only if the current instance hasn't been disposed yet.
            /// </summary>
            public void Dispose() {
                Action action = Interlocked.Exchange(ref _dispose, null);
                if (action == null) {
                    return;
                }
                action();
            }
        }
    }
}