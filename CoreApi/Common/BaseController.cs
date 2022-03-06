using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text;

namespace CoreApi.Common
{
    public static class HEADER_KEYS {
        public static readonly string API_KEY = "session_token";
    }
    [Controller]
    public class BaseController : ControllerBase
    {
        protected string __ControllerName;
        private ILogger __Logger;
        private string __TraceId;
        protected bool __LoadConfigSuccess = false;
        public string ControllerName { get => __ControllerName; }
        public bool LoadConfigSuccess { get => __LoadConfigSuccess; }
        public BaseController()
        {
            __Logger = Log.Logger;
            __TraceId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
            __ControllerName = "BaseController";
        }
        [NonAction]
        public virtual void LoadConfig()
        {
            __LoadConfigSuccess = true;
        }
        [NonAction]
        public void LogDebug(string Msg)
        {
            if (Msg != "") {
                StringBuilder msg = new StringBuilder($"Controller: { __ControllerName }, TraceId: { __TraceId }");
                msg.Append(", ").Append(Msg);
                __Logger.Debug(msg.ToString());
            }
        }
        [NonAction]
        public void LogInformation(string Msg)
        {
            if (Msg != "") {
                StringBuilder msg = new StringBuilder($"Controller: { __ControllerName }, TraceId: { __TraceId }");
                msg.Append(", ").Append(Msg);
                __Logger.Information(msg.ToString());
            }
        }
        [NonAction]
        public void LogWarning(string Msg)
        {
            if (Msg != "") {
                StringBuilder msg = new StringBuilder($"Controller: { __ControllerName }, TraceId: { __TraceId }");
                msg.Append(", ").Append(Msg);
                __Logger.Warning(msg.ToString());
            }
        }
        [NonAction]
        public void LogError(string Msg)
        {
            if (Msg != "") {
                StringBuilder msg = new StringBuilder($"Controller: { __ControllerName }, TraceId: { __TraceId }");
                msg.Append(", ").Append(Msg);
                __Logger.Error(msg.ToString());
            }
        }
    }
}