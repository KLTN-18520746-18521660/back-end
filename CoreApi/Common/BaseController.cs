using CoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Diagnostics;
using System.Text;

namespace CoreApi.Common
{
    public static class HEADER_KEYS {
        public static readonly string API_KEY = "session_token";
    }
    [Controller]
    [Produces("application/json")]
    public class BaseController : ControllerBase
    {
        private ILogger __Logger;
        protected string __ControllerName;
        protected string __TraceId;
        protected BaseConfig __BaseConfig;
        protected bool __LoadConfigSuccess = false;
        public string ControllerName { get => __ControllerName; }
        public string TraceId { get => __TraceId; }
        public bool LoadConfigSuccess { get => __LoadConfigSuccess; }
        public BaseController(BaseConfig _BaseConfig)
        {
            __Logger = Log.Logger;
            __TraceId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
            __BaseConfig = _BaseConfig;
            __ControllerName = "BaseController";
            __BaseConfig.SetTraceId(__TraceId);
        }
        [NonAction]
        public virtual void LoadConfig()
        {
            __LoadConfigSuccess = true;
        }
        [NonAction]
        protected virtual string CreateLogMessage(string Msg)
        {
            StringBuilder msg = new StringBuilder($"Controller: { __ControllerName }, TraceId: { __TraceId }");
            msg.Append(", ").Append(Msg);
            return msg.ToString();
        }
        [NonAction]
        protected virtual void LogDebug(string Msg)
        {
            if (Msg != string.Empty) {
                __Logger.Debug(CreateLogMessage(Msg));
            }
        }
        [NonAction]
        protected virtual void LogInformation(string Msg)
        {
            if (Msg != string.Empty) {
                __Logger.Information(CreateLogMessage(Msg));
            }
        }
        [NonAction]
        protected virtual void LogWarning(string Msg)
        {
            if (Msg != string.Empty) {
                __Logger.Warning(CreateLogMessage(Msg));
            }
        }
        [NonAction]
        protected virtual void LogError(string Msg)
        {
            if (Msg != string.Empty) {
                __Logger.Error(CreateLogMessage(Msg));
            }
        }
        [NonAction]
        public bool GetHeader(string HeaderKey, out string HeaderValue)
        {
            HeaderValue = string.Empty;
            if (!Request.Headers.ContainsKey(HeaderKey)) {
                return false;
            } else {
                HeaderValue = Request.Headers[HeaderKey];
                return true;
            }
        }
        [NonAction]
        protected ObjectResult Problem(int statusCode, string msg)
        {
            ObjectResult obj = new(new JObject(){
                { "status", statusCode },
                { "message", msg }
            });
            obj.StatusCode = statusCode;
            return obj;
        }
        [NonAction]
        protected ObjectResult Ok(int statusCode, string message, JObject data = default)
        {
            var respBody = new JObject(){
                { "status", statusCode },
                { "message", message },
                { "data", data },
            };
            if (data == default) {
                respBody.Remove("data");
            }
            ObjectResult obj = new (respBody);
            obj.StatusCode = statusCode;
            return obj;
        }
    }
}