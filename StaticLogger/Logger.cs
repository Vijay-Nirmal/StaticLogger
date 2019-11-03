using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace StaticLogger
{
    public static class Logger
    {
        private static ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();

        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();

        public static ILogger CreateLogger(string categoryName) => LoggerFactory.CreateLogger(categoryName);

        public static void UseLoggerFactory(this ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        public static void RemoveLoggerFactory()
        {
            LoggerFactory = null;
        }
    }
}
