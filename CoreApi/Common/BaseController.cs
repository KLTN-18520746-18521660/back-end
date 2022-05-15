using Common;
using CoreApi.Services;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

namespace CoreApi.Common
{
    public static class HEADER_KEYS {
        public static readonly string API_KEY       = "session_token";
        public static readonly string API_KEY_ADMIN = "session_token_admin";

        public static string[] GetAllowHeaders()
        {
            return typeof(HEADER_KEYS)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(e => e.FieldType == typeof(string))
                .Select(e => (string) e.GetValue(null))
                .ToArray();
        }
    }

    public static class HTTP_METHODS {
        public static readonly string GET       = "GET";
        public static readonly string PUT       = "PUT";
        public static readonly string POST      = "POST";
        public static readonly string DELETE    = "DELETE";

        public static string[] GetAllowMethods()
        {
            return typeof(HTTP_METHODS)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(e => e.FieldType == typeof(string))
                .Select(e => (string) e.GetValue(null))
                .ToArray();
        }
    }

    [Controller]
    [Produces("application/json")]
    public class BaseController : ControllerBase
    {
        private ILogger                         __Logger;
        private bool                            __IsAdminController;
        private string                          __TraceId;
        private string                          __ControllerName;
        private Mutex                           __DataContextMutex;
        private Dictionary<string, object>      __DataContext;
        protected BaseConfig                    __BaseConfig { get; private set; }

        #region Properties
        public string TraceId                   { get => __TraceId; protected set { __TraceId = value; } }
        public string ControllerName            { get => __ControllerName; protected set { __ControllerName = value; } }
        public string SessionTokenHeaderKey     { get; private set; }
        public bool IsAdminController {
            get => __IsAdminController;
            protected set {
                __IsAdminController = value;
                if (value) {
                    SessionTokenHeaderKey = HEADER_KEYS.API_KEY_ADMIN;
                } else {
                    SessionTokenHeaderKey = HEADER_KEYS.API_KEY;
                }
            }
        }
        #endregion

        public BaseController(BaseConfig _BaseConfig)
        {
            __Logger                = Log.Logger;
            __TraceId               = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
            __BaseConfig            = _BaseConfig;
            ControllerName        = "BaseController";
            __DataContext           = new Dictionary<string, object>();
            __DataContextMutex      = new Mutex();
            IsAdminController       = false;
        }
        #region Data context handle
        [NonAction]
        protected void SetValueToContext<T>(string Key, T Value)
        {
            __DataContextMutex.WaitOne();
            if (__DataContext.ContainsKey(Key)) {
                LogDebug($"Update context value, key: { Key }, value: { Value }");
                __DataContext[Key] = Value;
            } else {
                __DataContext.Add(Key, Value);
            }
            __DataContextMutex.ReleaseMutex();
        }
        protected (T, bool) GetValueFromContext<T>(string Key)
        {
            T RetVal    = default;
            var Ok      = false;
            __DataContextMutex.WaitOne();
            if (__DataContext.ContainsKey(Key)) {
                RetVal  = (T) System.Convert.ChangeType(__DataContext[Key], typeof(T));
                Ok      = true;
            }
            __DataContextMutex.ReleaseMutex();
            return (RetVal, Ok);
        }
        #endregion
        [NonAction]
        protected T GetConfigValue<T>(CONFIG_KEY ConfigKey, SUB_CONFIG_KEY SubConfigKey)
        {
            var Error               = string.Empty;
            var CombinedConfigKey   = $"@{ DEFAULT_BASE_CONFIG.ConfigKeyToString(ConfigKey) }_{ DEFAULT_BASE_CONFIG.SubConfigKeyToString(SubConfigKey) }";
            try {
                var (RetVal, GetOk) = GetValueFromContext<T>(CombinedConfigKey);
                if (GetOk) {
                    return RetVal;
                }
                (RetVal, Error)    = __BaseConfig.GetConfigValue<T>(ConfigKey, SubConfigKey);
                SetValueToContext<T>(CombinedConfigKey, RetVal);
                return RetVal;
            } catch (Exception e) {
                StringBuilder Msg = new StringBuilder(e.ToString());
                if (Error != e.Message && Error != string.Empty) {
                    Msg.Append($" && Error: { Error }");
                }
                throw new Exception(
                    "Get config value failed,"
                    + $"config_key: { ConfigKey }, sub_config_key: { SubConfigKey }, "
                    + $"message: { Msg }"
                );
            }
        }
        [NonAction]
        protected virtual string CreateLogMessage(string Msg, params string[] Params)
        {
            StringBuilder _Msg = new StringBuilder($"Controller: { ControllerName }, TraceId: { __TraceId }");
            _Msg.Append(", ").Append(Msg);
            foreach(var Param in Params) {
                _Msg.Append(", ").Append(Param);
            }
            return _Msg.ToString();
        }
        [NonAction]
        protected virtual void LogDebug(string Msg, params string[] Params)
        {
            if (Msg != string.Empty) {
                __Logger.Debug(CreateLogMessage(Msg, Params));
            }
        }
        [NonAction]
        protected virtual void LogInformation(string Msg, params string[] Params)
        {
            if (Msg != string.Empty) {
                __Logger.Information(CreateLogMessage(Msg, Params));
            }
        }
        [NonAction]
        protected virtual void LogWarning(string Msg, params string[] Params)
        {
            if (Msg != string.Empty) {
                __Logger.Warning(CreateLogMessage(Msg, Params));
            }
        }
        [NonAction]
        protected virtual void LogError(string Msg, params string[] Params)
        {
            if (Msg != string.Empty) {
                __Logger.Error(CreateLogMessage(Msg, Params));
            }
        }
        [NonAction]
        public void AddHeader(string HeaderKey, string Value)
        {
            Response.Headers.Add(HeaderKey, Value);
        }
        [NonAction]
        public string GetHeader(string HeaderKey)
        {
            return Request.Headers[HeaderKey];
        }
        [NonAction]
        public string GetValueFromCookie(string Key)
        {
            return Request.Cookies[Key];
        }
        [NonAction]
        public CookieOptions GetCookieOptionsForDelete(string Path = default,
                                                       SameSiteMode SameSite = SameSiteMode.Unspecified)
        {
            return new CookieOptions(){
                SameSite    = (SameSite  == SameSiteMode.Unspecified) ? SameSiteMode.Strict : SameSite,
                Expires     = new DateTime(1970, 1, 1, 0, 0, 0),
                Path        = (Path      == default) ? "/" : Path,
            };
        }
        [NonAction]
        public CookieOptions GetCookieOptions(DateTime Expires = default,
                                              string Path = default,
                                              SameSiteMode SameSite = SameSiteMode.Unspecified)
        {
            return new CookieOptions(){
                SameSite    = (SameSite  == SameSiteMode.Unspecified) ? SameSiteMode.Strict : SameSite,
                Expires     = (Expires   == default) ? DateTime.UtcNow.AddDays(365) : Expires,
                Path        = (Path      == default) ? "/" : Path,
            };
        }
        [NonAction]
        protected string MessageToCode(string Message)
        {
            return Message;
            // Message contains("not found") --> "NOT_FOUND{ $entity }", "NOT_FOUND{ $entity }"
        }
        [NonAction]
        protected ObjectResult Problem(int StatusCode, string Message, JObject Data = default)
        {
            var RespBody = new JObject(){
                { "status",         StatusCode },
                { "message",        Message },
                { "message_code",   MessageToCode(Message) },
                { "data",           Data },
            };
            if (Data == default) {
                RespBody.Remove("data");
            }
            ObjectResult Obj = new (RespBody);
            Obj.StatusCode = StatusCode;
            if (StatusCode == (int) StatusCodes.Status401Unauthorized) {
                Response.Cookies.Append(SessionTokenHeaderKey, string.Empty, GetCookieOptionsForDelete());
            }
            return Obj;
        }
        [NonAction]
        protected ObjectResult Ok(int StatusCode, string Message, JObject Data = default)
        {
            var RespBody = new JObject(){
                { "status",         StatusCode },
                { "message",        Message },
                { "message_code",   MessageToCode(Message) },
                { "data",           Data },
            };
            if (Data == default) {
                RespBody.Remove("data");
            }
            ObjectResult Obj = new (RespBody);
            Obj.StatusCode = StatusCode;
            return Obj;
        }

        #region Common functions
        [NonAction]
        protected async Task<(BaseModel, IActionResult)> GetSessionToken<T>(T SessionManager,
                                                                            string SessionToken,
                                                                            ErrorCodes[] IgnoreErrorCodes = default)
            where T : BaseTransientService
        {
            #region Get config values
            var ExpiryTime      = IsAdminController
                ? GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME)
                : GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
            var ExtensionTime   = IsAdminController
                ? GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME)
                : GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
            #endregion

            #region Get session token
            if (SessionToken == default) {
                LogDebug($"Missing header authorization.");
                return (default, Problem(401, "Missing header authorization."));
            }

            if (!CommonValidate.IsValidSessionToken(SessionToken)) {
                LogDebug($"Invalid header authorization.");
                return (default, Problem(401, "Invalid header authorization."));
            }
            #endregion

            #region Find session for use
            if (IgnoreErrorCodes == default) {
                IgnoreErrorCodes = new ErrorCodes[]{};
            }

            BaseModel Session = default;
            ErrorCodes Error = ErrorCodes.NO_ERROR;
            if (IsAdminController) {
                (Session, Error) = await (SessionManager as SessionAdminUserManagement).FindSessionForUse(SessionToken, ExpiryTime, ExtensionTime);
            } else {
                (Session, Error) = await (SessionManager as SessionSocialUserManagement).FindSessionForUse(SessionToken, ExpiryTime, ExtensionTime);
            }

            if (Error != ErrorCodes.NO_ERROR) {
                foreach (var It in IgnoreErrorCodes) {
                    if (It == Error) {
                        LogDebug($"Get session ignore error, error: { Error }");
                        return (Session, default);
                    }
                }
                if (Error == ErrorCodes.NOT_FOUND) {
                    LogDebug($"Session not found, { SessionTokenHeaderKey }: { SessionToken.Substring(0, 15) }");
                    return (default, Problem(401, "Session not found."));
                }
                if (Error == ErrorCodes.SESSION_HAS_EXPIRED) {
                    LogDebug($"Session has expired, { SessionTokenHeaderKey }: { SessionToken.Substring(0, 15) }");
                    return (default, Problem(401, "Session has expired."));
                }
                if (Error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                    LogWarning($"User has been locked, { SessionTokenHeaderKey }: { SessionToken.Substring(0, 15) }");
                    return (default, Problem(423, "You have been locked."));
                }
                if (Error == ErrorCodes.PASSWORD_IS_EXPIRED) {
                    LogWarning($"Password is expired and required to change, { SessionTokenHeaderKey }: { SessionToken.Substring(0, 15) }");
                    AddHeader("Location", "/profile/change-password");
                    return (default, Problem(301, "Password is expired, you must change password."));
                }
                throw new Exception($"FindSessionForUse Failed. ErrorCode: { Error }");
            }
            #endregion

            return (Session, default);
        }
        [NonAction]
        protected (string[], IActionResult) ValidateStatusParams(string Status, StatusType[] NotAllowStatus)
        {
            string[] StatusArr = Status == default ? default : Status.Split(',');
            if (Status != default) {
                foreach (var StatusStr in StatusArr) {
                    var _StatusType = EntityStatus.StatusStringToType(StatusStr);
                    if (_StatusType == default || NotAllowStatus.Contains(_StatusType)) {
                        return (default, Problem(400, $"Invalid status: { StatusStr }."));
                    }
                }
            }
            return (StatusArr, default);
        }
        [NonAction]
        protected ((string, bool)[], IActionResult) ValidateOrderParams(Models.OrderModel Orders, string[] AllowOrderParams)
        {
            if (!Orders.IsValid()) {
                return (default, Problem(400, "Invalid order params."));
            }
            var CombineOrders = Orders.GetOrders();
            foreach (var It in CombineOrders) {
                if (!AllowOrderParams.Contains(It.Item1)) {
                    return (default, Problem(400, $"Not allow order field: { It.Item1 }."));
                }
            }
            return (CombineOrders, default);
        }
    }
    #endregion
}