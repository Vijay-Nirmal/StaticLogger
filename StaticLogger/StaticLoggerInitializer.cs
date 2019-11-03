using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StaticLogger
{
    public class StaticLoggerInitializer : IHostedService
    {
        private readonly ILoggerFactory _loggerFactory;

        public StaticLoggerInitializer(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _loggerFactory.UseLoggerFactory();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.RemoveLoggerFactory();
            return Task.CompletedTask;
        }
    }
}
