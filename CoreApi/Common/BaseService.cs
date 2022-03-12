using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text;

namespace CoreApi.Common
{
    public class BaseService
    {
        protected string __ServiceName;
        private ILogger __Logger;
        private string __TraceId;
        public string ServiceName { get => __ServiceName; }
        public BaseService()
        {
            __Logger = Log.Logger;
            __TraceId = Activity.Current?.Id;
            __ServiceName = "BaseService";
        }
        public void LogDebug(string Msg)
        {
            if (Msg != "") {
                StringBuilder msg = new StringBuilder($"Service: { __ServiceName }, TraceId: { __TraceId }");
                msg.Append(", ").Append(Msg);
                __Logger.Debug(msg.ToString());
            }
        }
        public void LogInformation(string Msg)
        {
            if (Msg != "") {
                StringBuilder msg = new StringBuilder($"Service: { __ServiceName }, TraceId: { __TraceId }");
                msg.Append(", ").Append(Msg);
                __Logger.Information(msg.ToString());
            }
        }
        public void LogWarning(string Msg)
        {
            if (Msg != "") {
                StringBuilder msg = new StringBuilder($"Service: { __ServiceName }, TraceId: { __TraceId }");
                msg.Append(", ").Append(Msg);
                __Logger.Warning(msg.ToString());
            }
        }
        public void LogError(string Msg)
        {
            if (Msg != "") {
                StringBuilder msg = new StringBuilder($"Service: { __ServiceName }, TraceId: { __TraceId }");
                msg.Append(", ").Append(Msg);
                __Logger.Error(msg.ToString());
            }
        }
    }
}