using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace CoreApi.Controllers.Admin.Session
{
    [ApiController]
    [Route("/admin/session")]
    public class GetAdminUserByApikeyController : BaseController
    {
        #region Services
        private SessionAdminUserManagement __SessionAdminUserManagement;
        private BaseConfig __BaseConfig;
        #endregion

        #region Config Value
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetAdminUserByApikeyController(
            SessionAdminUserManagement _SessionAdminUserManagement,
            BaseConfig _BaseConfig
        ) : base() {
            __SessionAdminUserManagement = _SessionAdminUserManagement;
            __BaseConfig = _BaseConfig;
            __ControllerName = "GetAdminUserByApikey";
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

        [HttpGet("user")]
        public IActionResult GetSocialUserByApiKey()
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

                var user = session.User;
                LogInformation($"Get info user by apikey success, user_name: { user.UserName }");
                return Ok( new JObject(){
                    { "status", 200 },
                    { "user", user.GetJsonObject() },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.Message }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
