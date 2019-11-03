// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using FrameworkLogger = Microsoft.Extensions.Logging.ILogger;

namespace Serilog.Extensions.Provider
{
    internal class SerilogLogger : FrameworkLogger
    {
        private readonly ILogger _logger;
        private readonly IExternalScopeProvider _externalScopeProvider;
        private static readonly MessageTemplateParser _messageTemplateParser = new MessageTemplateParser();

        // It's rare to see large event ids, as they are category-specific
        private static readonly LogEventProperty[] _lowEventIdValues = Enumerable.Range(0, 48)
            .Select(n => new LogEventProperty("Id", new ScalarValue(n)))
            .ToArray();

        public SerilogLogger(ILogger logger = null, string name = null, IExternalScopeProvider externalScopeProvider = null)
        {
            _logger = logger;
            _externalScopeProvider = externalScopeProvider;

            if (name != null)
            {
                _logger = _logger.ForContext(Constants.SourceContextPropertyName, name);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel.ToSerilogLevel());
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _externalScopeProvider.Push(state);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var level = logLevel.ToSerilogLevel();
            if (!_logger.IsEnabled(level))
            {
                return;
            }

            var logger = _logger;
            string messageTemplate = null;

            var properties = new List<LogEventProperty>();

            if (state is IEnumerable<KeyValuePair<string, object>> structure)
            {
                foreach (var property in structure)
                {
                    if (string.Equals(property.Key, SerilogLoggerProvider._originalFormatPropertyName, StringComparison.OrdinalIgnoreCase) && property.Value is string value)
                    {
                        messageTemplate = value;
                    }
                    else if (property.Key.StartsWith("@", StringComparison.Ordinal))
                    {
                        if (logger.BindProperty(property.Key.Substring(1), property.Value, destructureObjects: true, out var destructured))
                            properties.Add(destructured);
                    }
                    else
                    {
                        if (logger.BindProperty(property.Key, property.Value, destructureObjects: false, out var bound))
                            properties.Add(bound);
                    }
                }

                var stateType = state.GetType();
                var stateTypeInfo = stateType.GetTypeInfo();
                // Imperfect, but at least eliminates `1 names
                if (messageTemplate == null && !stateTypeInfo.IsGenericType)
                {
                    messageTemplate = "{" + stateType.Name + ":l}";
                    if (logger.BindProperty(stateType.Name, AsLoggableValue(state, formatter), destructureObjects: false, out var stateTypeProperty))
                        properties.Add(stateTypeProperty);
                }
            }

            if (messageTemplate == null)
            {
                string propertyName = null;
                if (state != null)
                {
                    propertyName = "State";
                    messageTemplate = "{State:l}";
                }
                else if (formatter != null)
                {
                    propertyName = "Message";
                    messageTemplate = "{Message:l}";
                }

                if (propertyName != null && logger.BindProperty(propertyName, AsLoggableValue(state, formatter), false, out var property))
                {
                    properties.Add(property);
                }
            }

            if (eventId.Id != 0 || eventId.Name != null)
                properties.Add(CreateEventIdProperty(eventId));

            var parsedTemplate = _messageTemplateParser.Parse(messageTemplate ?? "");
            var logEvent = new LogEvent(DateTimeOffset.Now, level, exception, parsedTemplate, properties);

            if (!(_externalScopeProvider is null))
            {
                var stringBuilder = new StringBuilder();
                _externalScopeProvider.ForEachScope(
                    (activeScope, builder) =>
                    {

                        if (activeScope is IReadOnlyCollection<KeyValuePair<string, object>> activeScopeDictionary)
                        {
                            foreach (KeyValuePair<string, object> item in activeScopeDictionary)
                            {
                                logEvent.AddPropertyIfAbsent(new LogEventProperty(item.Key, new ScalarValue(Convert.ToString(item.Value, CultureInfo.InvariantCulture))));
                            }
                        }
                        else
                        {
                            builder.Append(" => ").Append(activeScope);
                        }
                    },
                    stringBuilder);
            }

            logger.Write(logEvent);
        }

        private static object AsLoggableValue<TState>(TState state, Func<TState, Exception, string> formatter)
        {
            object sobj = state;
            if (formatter != null)
                sobj = formatter(state, arg2: null);
            return sobj;
        }

        internal static LogEventProperty CreateEventIdProperty(EventId eventId)
        {
            var properties = new List<LogEventProperty>(2);

            if (eventId.Id != 0)
            {
                if (eventId.Id >= 0 && eventId.Id < _lowEventIdValues.Length)
                    // Avoid some allocations
                    properties.Add(_lowEventIdValues[eventId.Id]);
                else
                    properties.Add(new LogEventProperty("Id", new ScalarValue(eventId.Id)));
            }

            if (eventId.Name != null)
            {
                properties.Add(new LogEventProperty("Name", new ScalarValue(eventId.Name)));
            }

            return new LogEventProperty("EventId", new StructureValue(properties));
        }
    }
}