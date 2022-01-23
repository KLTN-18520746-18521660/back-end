
using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace coreApi.Common
{
    // [INFO] Handle log file in one line
    public class ExceptionEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Exception == null)
                return;
            LogEventProperty logEventProperty;
#if DEBUG
            logEventProperty = propertyFactory.CreateProperty(
                "EscapedException",
                Regex.Replace(logEvent.Exception.ToString(), Environment.NewLine, " ")
            );
#else
            logEventProperty = propertyFactory.CreateProperty(
                "EscapedException",
                "logEvent.Exception.ToString().Split(System.Environment.NewLine)[0]"
            );
#endif
            logEvent.AddPropertyIfAbsent(logEventProperty);
        }
    }
    public class MessageEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.MessageTemplate == null)
                return;

            var logEventProperty = propertyFactory.CreateProperty("EscapedMessage", Regex.Replace(logEvent.MessageTemplate.ToString(), Environment.NewLine, " "));
            logEvent.AddPropertyIfAbsent(logEventProperty);
        }
    }
}