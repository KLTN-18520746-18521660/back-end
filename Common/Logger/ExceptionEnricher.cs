using System;
using System.Text.RegularExpressions;
using System.Threading;
using Serilog.Core;
using Serilog.Events;
using System.Text;

namespace Common.Logger
{
    // [INFO] Handle log file in one line
    public class ExceptionEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ThreadId", Thread.CurrentThread.ManagedThreadId));
            if (logEvent.Exception == default)
                return;
            LogEventProperty logEventProperty;
#if DEBUG
            logEventProperty = propertyFactory.CreateProperty(
                "EscapedException",
                (new StringBuilder(Regex.Replace(logEvent.Exception.ToString(), Environment.NewLine, " ")).Append(Environment.NewLine)).ToString()
            );
#else
            logEventProperty = propertyFactory.CreateProperty(
                "EscapedException",
                (new StringBuilder(logEvent.Exception.ToString().Split(System.Environment.NewLine)[0]).Append(Environment.NewLine)).ToString()
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
            if (logEvent.MessageTemplate == default)
                return;

            var logEventProperty = propertyFactory.CreateProperty("EscapedMessage", Regex.Replace(logEvent.MessageTemplate.ToString(), Environment.NewLine, " "));
            logEvent.AddPropertyIfAbsent(logEventProperty);
        }
    }
}