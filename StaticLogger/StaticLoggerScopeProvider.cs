using Microsoft.Extensions.Logging;
using System;

namespace StaticLogger
{
    public class StaticLoggerScopeProvider : IExternalScopeProvider
    {
        public static readonly LoggerExternalScopeProvider ExternalScopeProvider = new LoggerExternalScopeProvider();

        public IDisposable Push(object state)
        {
            return ExternalScopeProvider.Push(state);
        }

        public void ForEachScope<TState>(Action<object, TState> callback, TState state)
        {
            ExternalScopeProvider.ForEachScope(callback, state);
        }
    }
}
