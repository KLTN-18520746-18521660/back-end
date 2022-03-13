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
    public static class STATUS_CODE_TITLE {
        public static readonly string BAD_REQUEST = "Bad Request";
    }
    [Controller]
    [Produces("application/json")]
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
        [NonAction]
        public bool GetHeader(string HeaderKey, out string HeaderValue)
        {
            HeaderValue = "";
            if (!Request.Headers.ContainsKey(HeaderKey)) {
                return false;
            } else {
                HeaderValue = Request.Headers[HeaderKey];
                return true;
            }
        }
        [NonAction]
        public ObjectResult Problem(int statusCode, object msg)
        {
            ObjectResult obj = new(new JObject(){
                { "status", statusCode },
                { "error", JToken.FromObject(msg) }
            });
            obj.StatusCode = statusCode;
            return obj;
        }
        [NonAction]
        public ObjectResult Ok(int statusCode, JObject body)
        {
            ObjectResult obj = new(body);
            obj.StatusCode = statusCode;
            return obj;
        }
    }
}