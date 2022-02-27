using System;
using System.Text.RegularExpressions;
using System.Threading;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Common.Logger
{
    // [INFO] Handle log file in one line
    public class ExceptionEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ThreadId", Thread.CurrentThread.ManagedThreadId));
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
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ThreadId", Thread.CurrentThread.ManagedThreadId));
            if (logEvent.MessageTemplate == null)
                return;

            var logEventProperty = propertyFactory.CreateProperty("EscapedMessage", Regex.Replace(logEvent.MessageTemplate.ToString(), Environment.NewLine, " "));
            logEvent.AddPropertyIfAbsent(logEventProperty);
        }
    }
}