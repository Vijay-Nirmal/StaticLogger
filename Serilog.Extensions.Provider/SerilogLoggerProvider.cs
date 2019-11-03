// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FrameworkLogger = Microsoft.Extensions.Logging.ILogger;

namespace Serilog.Extensions.Provider
{
    /// <summary>
    /// An <see cref="ILoggerProvider"/> that pipes events through Serilog.
    /// </summary>
    [ProviderAlias("Serilog")]
    public sealed class SerilogLoggerProvider : ILoggerProvider
    {
        internal const string _originalFormatPropertyName = "{OriginalFormat}";
        internal const string _scopePropertyName = "Scope";

        private readonly ILogger _logger;
        private readonly IExternalScopeProvider _externalScopeProvider;

        /// <summary>
        /// Construct a <see cref="SerilogLoggerProvider"/>.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        public SerilogLoggerProvider(IConfiguration configuration) : this(configuration, externalScopeProvider: null) { }

        public SerilogLoggerProvider(IConfiguration configuration, IExternalScopeProvider externalScopeProvider)
        {
            _logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            _externalScopeProvider = externalScopeProvider;
        }

        /// <summary>
        /// Create Logger Instance
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public FrameworkLogger CreateLogger(string categoryName)
        {
            return new SerilogLogger(_logger, categoryName, _externalScopeProvider);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Noting to Dispose
        }
    }
}
