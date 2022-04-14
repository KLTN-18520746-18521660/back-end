using CoreApi.Common.Interface;
using DatabaseAccess.Context;
using Serilog;
using System;
using System.Text;

namespace CoreApi.Common
{
    public class BaseSingletonService : IBaseService
    {
        private ILogger __Logger;
        protected string __ServiceName;
        protected IServiceProvider __ServiceProvider;
        public string ServiceName { get => __ServiceName; }
        public BaseSingletonService(IServiceProvider _IServiceProvider)
        {
            __Logger = Log.Logger;
            __ServiceProvider = _IServiceProvider;
            __ServiceName = "BaseSingletonService";
        }
        protected virtual string CreateLogMessage(string Msg)
        {
            string msgFormat = $"Service: { __ServiceName }";
            StringBuilder msg = new StringBuilder(msgFormat);
            msg.Append(", ").Append(Msg);
            return msg.ToString();
        }
        protected virtual void LogDebug(string Msg)
        {
            if (Msg != string.Empty) {
                __Logger.Debug(CreateLogMessage(Msg));
            }
        }
        protected virtual void LogInformation(string Msg)
        {
            if (Msg != string.Empty) {
                __Logger.Information(CreateLogMessage(Msg));
            }
        }
        protected virtual void LogWarning(string Msg)
        {
            if (Msg != string.Empty) {
                __Logger.Warning(CreateLogMessage(Msg));
            }
        }
        protected virtual void LogError(string Msg)
        {
            if (Msg != string.Empty) {
                __Logger.Error(CreateLogMessage(Msg));
            }
        }
    }
}