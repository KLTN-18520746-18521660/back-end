using DatabaseAccess.Context;
using Serilog;
using System;
using System.Text;

namespace CoreApi.Common
{
    public class BaseService
    {
        private ILogger __Logger;
        protected string __ServiceName;
        protected string __TraceId;
        protected DBContext __DBContext;
        protected IServiceProvider __ServiceProvider;
        public string ServiceName { get => __ServiceName; }
        public string TraceId { get => __TraceId; }
        public BaseService(DBContext _DBContext,
                           IServiceProvider _IServiceProvider)
        {
            __Logger = Log.Logger;
            __TraceId = "";
            __DBContext = _DBContext;
            __ServiceProvider = _IServiceProvider;
            __ServiceName = "BaseService";
        }
        public virtual void SetTraceId(string TraceId)
        {
            __TraceId = TraceId;
        }
        protected virtual string CreateLogMessage(string Msg)
        {
            string msgFormat = TraceId == string.Empty ? $"Service: { __ServiceName }" : $"Service: { __ServiceName }, TraceId: { __TraceId }";
            StringBuilder msg = new StringBuilder(msgFormat);
            msg.Append(", ").Append(Msg);
            return msg.ToString();
        }
        public virtual void LogDebug(string Msg)
        {
            if (Msg != "") {
                __Logger.Debug(CreateLogMessage(Msg));
            }
        }
        public virtual void LogInformation(string Msg)
        {
            if (Msg != "") {
                __Logger.Information(CreateLogMessage(Msg));
            }
        }
        public virtual void LogWarning(string Msg)
        {
            if (Msg != "") {
                __Logger.Warning(CreateLogMessage(Msg));
            }
        }
        public virtual void LogError(string Msg)
        {
            if (Msg != "") {
                __Logger.Error(CreateLogMessage(Msg));
            }
        }
    }
}