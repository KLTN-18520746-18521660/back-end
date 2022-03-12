using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApi.Controllers.Admin.Session
{
    [ApiController]
    [Route("/admin/session")]
    public class GetAllSessionAdminUserController : BaseController
    {
        #region Services
        private BaseConfig __BaseConfig;
        private SessionAdminUserManagement __SessionAdminUserManagement;
        #endregion

        #region Config Value
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minutes
        #endregion

        public GetAllSessionAdminUserController(
            BaseConfig _BaseConfig,
            SessionAdminUserManagement _SessionAdminUserManagement
        ) : base() {
            __BaseConfig = _BaseConfig;
            __SessionAdminUserManagement = _SessionAdminUserManagement;
            __ControllerName = "GetAllSessionAdminUser";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                EXTENSION_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, "extension_time", out Error);
                EXPIRY_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, "expiry_time", out Error);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.Message);
                if (Error != e.Message && Error != "") {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value fail, message: { msg }");
            }
        }

        [HttpGet("")]
        public IActionResult GetAllSession()
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            try {
                #region Get session token
                string sessionToken = "";
                if (!GetHeader(HEADER_KEYS.API_KEY, out sessionToken)) {
                    LogDebug($"Missing header authorization.");
                    return Problem(403, "Missing header authorization.");
                }

                if (!CoreApi.Common.Utils.IsValidSessionToken(sessionToken)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionAdminUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;

                if (!__SessionAdminUserManagement.FindSessionForUse(sessionToken, EXPIRY_TIME, EXTENSION_TIME, out session, out error)) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(400, "Session not found.");
                    }
                    if (error == ErrorCodes.SESSION_HAS_EXPIRED) {
                        LogInformation($"Session has expired, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(401, "Session has expired.");
                    }
                    throw new Exception("Internal Server Error. FindSessionForUse Failed.");
                }
                #endregion

                #region Get all sessions
                var user = session.User;
                List<SessionAdminUser> allSessionOfUser = null;
                if (!__SessionAdminUserManagement.GetAllSessionOfUser(user.Id, out allSessionOfUser, out error)) {
                    throw new Exception("Internal Server Error. GetAllAdminUserSessions Failed.");
                }

                List<JObject> rawReturn = new();
                allSessionOfUser.ForEach(e => rawReturn.Add(e.GetJsonObject()));
                var ret = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(rawReturn));
                #endregion

                LogDebug($"Get all session success, user_name: { user.UserName }");
                return Ok( new JObject(){
                    { "status", 200 },
                    { "sessions", ret },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.Message }");
                return Problem(500, "Internal Server error.");
            }
        }
        [HttpGet("{session_token}")]
        public IActionResult GetAllSession(string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            try {
                #region Get session token
                string sessionToken = "";
                if (!GetHeader(HEADER_KEYS.API_KEY, out sessionToken)) {
                    LogDebug($"Missing header authorization.");
                    return Problem(403, "Missing header authorization.");
                }

                if (!CoreApi.Common.Utils.IsValidSessionToken(sessionToken)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Check param session_token
                if (!CoreApi.Common.Utils.IsValidSessionToken(session_token)) {
                    return Problem(400, "Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionAdminUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;

                if (!__SessionAdminUserManagement.FindSessionForUse(sessionToken, EXPIRY_TIME, EXTENSION_TIME, out session, out error)) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(400, "Session not found.");
                    }
                    if (error == ErrorCodes.SESSION_HAS_EXPIRED) {
                        LogInformation($"Session has expired, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(401, "Session has expired.");
                    }
                    throw new Exception("Internal Server Error. FindSessionForUse Failed.");
                }
                #endregion

                #region Get all sessions
                var user = session.User;
                SessionAdminUser ret = null;
                if (!__SessionAdminUserManagement.FindSession(session_token, out ret, out error)) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { session_token.Substring(0, 15) }");
                        return Problem(404, "Session not found.");
                    }
                    throw new Exception("Internal Server Error. FindSessionAdminUser Failed.");
                }
                if (ret.User.Id != user.Id) {
                    LogDebug($"Session not found, session_token: { session_token.Substring(0, 15) }");
                    return Problem(404, "Session not found.");
                }
                #endregion

                LogDebug($"Get session success, user_name: { user.UserName }, session_token: { session_token.Substring(0, 15) }");
                return Ok( new JObject(){
                    { "status", 200 },
                    { "sessions", ret.GetJsonObject() },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.Message }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
