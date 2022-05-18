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
using Serilog.Events;
using System.Runtime.CompilerServices;

namespace CoreApi.Common.Base
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
        private static readonly int                             MAX_SIZE_OF_LOG_PARAMS = 15;  // It can not change in runtime
        private ILogger                                         __Logger;
        private bool                                            __IsAdminController;
        private string                                          __TraceId;
        private string                                          __ControllerName;
        private string                                          __RunningFunction;
        private Mutex                                           __DataContextMutex;
        private Dictionary<string, object>                      __DataContext;
        private List<(string PName, object PValue)>     __LogParams;

        #region Properties
        protected BaseConfig                    __BaseConfig { get; private set; }
        public string TraceId                   { get => __TraceId; private set { __TraceId = value; } }
        public string ControllerName            { get => __ControllerName; private set { __ControllerName = value; } }
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

        public BaseController(BaseConfig _BaseConfig, bool _IsAdminController = false)
        {
            __Logger                = Log.Logger;
            __TraceId               = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
            __BaseConfig            = _BaseConfig;
            // __ControllerName        = "BaseController";
            __ControllerName        = Utils.GetHandlerNameFromClassName(this.GetType().Name);
            __DataContext           = new Dictionary<string, object>();
            __LogParams             = new List<(string, object)>();
            __DataContextMutex      = new Mutex();
            __RunningFunction       = "Invalid";
            IsAdminController       = _IsAdminController;
        }
        #region Data context handle
        [NonAction]
        protected void SetValueToContext<T>(string Key, T Value)
        {
            __DataContextMutex.WaitOne();
            if (__DataContext.ContainsKey(Key)) {
                WriteLog(LOG_LEVEL.DEBUG, false, $"Update context value", Param("key", Key), Param("value", Value));
                __DataContext[Key] = Value;
            } else {
                __DataContext.Add(Key, Value);
            }
            __DataContextMutex.ReleaseMutex();
        }
        protected (bool IsOK, T Value) GetValueFromContext<T>(string Key)
        {
            T RetVal    = default;
            var Ok      = false;
            __DataContextMutex.WaitOne();
            if (__DataContext.ContainsKey(Key)) {
                RetVal  = (T) System.Convert.ChangeType(__DataContext[Key], typeof(T));
                Ok      = true;
            }
            __DataContextMutex.ReleaseMutex();
            return (Ok, RetVal);
        }
        #endregion
        #region Base config
        [NonAction]
        protected T GetConfigValue<T>(CONFIG_KEY ConfigKey, SUB_CONFIG_KEY SubConfigKey)
        {
            var Error               = string.Empty;
            var CombinedConfigKey   = $"@{ DEFAULT_BASE_CONFIG.ConfigKeyToString(ConfigKey) }_{ DEFAULT_BASE_CONFIG.SubConfigKeyToString(SubConfigKey) }";
            try {
                var (GetOk, RetVal) = GetValueFromContext<T>(CombinedConfigKey);
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
        #endregion
        #region Loging
        [NonAction]
        protected virtual string Param<T>(string PName, T PValue)
        {
            return Utils.ParamsToLog(PName, PValue);
        }
        [NonAction]
        protected virtual void AddLogParam(string PName, object PValue)
        {
            if (__LogParams.Count >= MAX_SIZE_OF_LOG_PARAMS) {
                WriteLog(LOG_LEVEL.ERROR, false, "Log params exceed max size",
                    Param("max_size",           MAX_SIZE_OF_LOG_PARAMS),
                    Param("log_params_size",    __LogParams.Count)
                );
                return;
            }
            __LogParams.Add((PName, PValue));
        }
        [NonAction]
        protected virtual string CreateLogMessage(string Msg, params string[] Params)
        {
            var _Msg        = new StringBuilder(string.Format("TraceId: {0}, Handler: {1}.{2}", TraceId, ControllerName, __RunningFunction));
            var ParamsStr   = string.Join(", ", Params);
            _Msg.Append(", Message: ").Append(Msg);
            if (ParamsStr != string.Empty) {
                _Msg.Append(", ").Append(ParamsStr);
            }
            return _Msg.ToString();
        }
        [NonAction]
        protected virtual string CreateLogParamsMessage()
        {
            var Params = __LogParams.Select(e => Param(e.PName, e.PValue)).ToArray();
            return string.Join(", ", Params);
        }
        [NonAction]
        protected virtual void WriteLog(LOG_LEVEL Level, bool LogParams, string Message, params string[] CustomParams)
        {
            if (Message == string.Empty) {
                return;
            }
            var CustomParamsStr = new StringBuilder(string.Join(", ", CustomParams));
            if (LogParams && __LogParams.Count != 0) {
                if (CustomParams.Length != 0) {
                    CustomParamsStr.Append(", ");
                }
                CustomParamsStr.Append(CreateLogParamsMessage());
            }
            __Logger.Write((LogEventLevel) Level, CreateLogMessage(Message, CustomParamsStr.ToString()));
        }
        #endregion
        #region Common functions
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
        protected virtual void SetTraceIdForServices(params BaseTransientService[] Services)
        {
            foreach (var Service in Services) {
                Service.SetTraceId(TraceId);
            }
        }
        [NonAction]
        protected virtual void SetRunningFunction([CallerMemberName] string MemberName = "")
        {
            __RunningFunction = MemberName;
        }
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
                return (default, Problem(401, "Missing header authorization.", default, LOG_LEVEL.DEBUG));
            }

            if (!CommonValidate.IsValidSessionToken(SessionToken)) {
                return (default, Problem(401, "Invalid header authorization.", default, LOG_LEVEL.DEBUG));
            }
            #endregion

            #region Find session for use
            if (IgnoreErrorCodes == default) {
                IgnoreErrorCodes = new ErrorCodes[]{};
            }

            BaseModel Session   = default;
            ErrorCodes Error    = ErrorCodes.NO_ERROR;
            if (IsAdminController) {
                (Session, Error) = await (SessionManager as SessionAdminUserManagement).FindSessionForUse(SessionToken, ExpiryTime, ExtensionTime);
            } else {
                (Session, Error) = await (SessionManager as SessionSocialUserManagement).FindSessionForUse(SessionToken, ExpiryTime, ExtensionTime);
            }

            AddLogParam(SessionTokenHeaderKey, SessionToken);
            if (Error != ErrorCodes.NO_ERROR) {
                foreach (var It in IgnoreErrorCodes) {
                    if (It == Error) {
                        WriteLog(LOG_LEVEL.DEBUG, false, $"Get session success ignore error", Param("ignore_error_code", Error));
                        return (Session, default);
                    }
                }
                if (Error == ErrorCodes.NOT_FOUND) {
                    return (default, Problem(401, "Session not found.", default, LOG_LEVEL.DEBUG));
                }
                AddLogParam(
                    "user_name",
                    IsAdminController ? (Session as SessionAdminUser).User.UserName : (Session as SessionSocialUser).User.UserName
                );
                if (Error == ErrorCodes.SESSION_HAS_EXPIRED) {
                    return (default, Problem(401, "Session has expired.", default, LOG_LEVEL.DEBUG));
                }
                if (Error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                    return (default, Problem(423, "You have been locked."));
                }
                if (Error == ErrorCodes.PASSWORD_IS_EXPIRED) {
                    AddHeader("Location", IsAdminController ? "/admin/profile/change-password" : "/profile/change-password");
                    return (default, Problem(301, "Password is expired, you must change password."));
                }
                AddLogParam("get_session_token_error_code", Error.ToString());
                throw new Exception($"FindSessionForUse failed");
            }
            #endregion


            AddLogParam(
                "user_name",
                IsAdminController ? (Session as SessionAdminUser).User.UserName : (Session as SessionSocialUser).User.UserName
            );
            WriteLog(LOG_LEVEL.DEBUG, true, $"Get session success without error.");
            return (Session, default);
        }
        [NonAction]
        protected (string[], IActionResult) ValidateStatusParams(string Status, StatusType[] NotAllowStatus)
        {
            string[] StatusArr = Status == default ? new string[]{} : Status.Split(',');
            if (Status != default) {
                foreach (var StatusStr in StatusArr) {
                    var _StatusType = EntityStatus.StatusStringToType(StatusStr);
                    if (_StatusType == default || NotAllowStatus.Contains(_StatusType)) {
                        AddLogParam("not_allow_status",    NotAllowStatus);
                        AddLogParam("invalid_status",      StatusStr);
                        return (default, Problem(400, $"Invalid status.", default, LOG_LEVEL.DEBUG));
                    }
                }
            }
            return (StatusArr, default);
        }
        [NonAction]
        protected ((string, bool)[], IActionResult) ValidateOrderParams(Models.OrderModel Orders, string[] AllowOrderParams)
        {
            if (!Orders.IsValid()) {
                AddLogParam("orders_param", Orders);
                return (default, Problem(400, "Invalid order params.", default, LOG_LEVEL.DEBUG));
            }
            var CombineOrders = Orders.GetOrders();
            foreach (var It in CombineOrders) {
                if (!AllowOrderParams.Contains(It.Item1)) {
                    AddLogParam("allow_order_params",  AllowOrderParams);
                    AddLogParam("invalid_order_param", It.Item1);
                    return (default, Problem(400, $"Not allow order field."));
                }
            }
            return (CombineOrders, default);
        }
        #endregion
        #region Handler rest response
        [NonAction]
        protected string MessageToCode(string Message)
        {
            return RESPONSE_MESSAGES.INVALID_REST_MESSAGE.CODE;
        }
        [NonAction]
        protected ObjectResult Problem(int StatusCode, string Message, JObject Data = default, LOG_LEVEL Level = LOG_LEVEL.WARNING)
        {
            var MessageCode = MessageToCode(Message);
            var RespBody    = new JObject(){
                { "status",         StatusCode },
                { "message",        Message },
                { "message_code",   MessageCode },
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
            WriteLog(Level, true, MessageCode);
            return Obj;
        }
        [NonAction]
        protected ObjectResult Ok(int StatusCode, string Message, JObject Data = default, LOG_LEVEL Level = LOG_LEVEL.INFO)
        {
            var MessageCode = MessageToCode(Message);
            var RespBody = new JObject(){
                { "status",         StatusCode },
                { "message",        Message },
                { "message_code",   MessageCode },
                { "data",           Data },
            };
            if (Data == default) {
                RespBody.Remove("data");
            }
            ObjectResult Obj = new (RespBody);
            Obj.StatusCode = StatusCode;
            WriteLog(Level, true, MessageCode);
            return Obj;
        }
        #endregion
    }
}
