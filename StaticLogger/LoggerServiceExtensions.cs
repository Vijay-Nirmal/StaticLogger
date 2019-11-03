using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Provider;

namespace StaticLogger
{
    public static class LoggerServiceExtensions
    {
        public static IServiceCollection AddLogger(this IServiceCollection services)
        {
            services.AddSingleton<IHostedService, StaticLoggerInitializer>();
            services.AddSingleton<IExternalScopeProvider, StaticLoggerScopeProvider>();

            services.AddLogging(builder =>
            {
                builder.AddSerilog();
            });

            return services;
        }
    }
}
