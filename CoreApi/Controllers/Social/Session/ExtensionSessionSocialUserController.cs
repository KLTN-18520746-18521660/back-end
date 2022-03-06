using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseAccess.Context;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CoreApi.Common;
using CoreApi.Common.Interface;
// using System.Data.Entity;
using System.Diagnostics;
using System.Text;
// using System.Text.Json;

namespace CoreApi.Controllers.Social.Session
{
    [ApiController]
    [Route("/session/extension")]
    public class ExtensionSessionSocialUserController : BaseController
    {
        private DBContext __DBContext;
        private IBaseConfig __BaseConfig;
        /////////// CONFIG VALUE ///////////
        private int EXTENSION_TIME; // minute
        private int EXPIRY_TIME; // minute
        ////////////////////////////////////
        public ExtensionSessionSocialUserController(
            DBContext _DBContext,
            IBaseConfig _BaseConfig
        ) : base() {
            __DBContext = _DBContext;
            __BaseConfig = _BaseConfig;
            __ControllerName = "ExtensionSessionSocialUser";
            LoadConfig();
        }
        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                EXTENSION_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, "extension_time", Error);
                LogWarning(Error);
                EXPIRY_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, "expiry_time", Error);
                LogWarning(Error);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                StringBuilder msg = new StringBuilder(e.Message);
                if (Error != e.Message && Error != "") {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value fail, message: { msg }");
            }
        }

        [HttpPost]
        public IActionResult ExtensionSession()
        {
            if (!LoadConfigSuccess) {
                return Problem(
                    detail: "Internal Server error.",
                    statusCode: 500,
                    instance: "/session/extension",
                    title: "Internal Server Error",
                    type: "/report"
                );
            }
            try {
                if (!Request.Headers.ContainsKey(HEADER_KEYS.API_KEY)) {
                    LogDebug($"Missing header authorization.");
                    return Problem(
                        detail: "Missing header authorization.",
                        statusCode: 403,
                        instance: "/session/extension",
                        title: "Bad Request",
                        type: "/help"
                    );
                }

                string sessionToken = Request.Headers[HEADER_KEYS.API_KEY];
                if (!CoreApi.Common.Utils.IsValidSessionToken(sessionToken)) {
                    return Problem(
                        detail: "Invalid header authorization.",
                        statusCode: 403,
                        instance: "/session/extension",
                        title: "Bad Request",
                        type: "/help"
                    );
                }
                var sessions = __DBContext.SessionSocialUsers
                                .Where(e => e.SessionToken == sessionToken)
                                .ToList();

                if (sessions.Count < 1) {
                    LogDebug($"Session not found, session_token: { sessionToken.Substring(0, 15) }");
                    return Problem(
                        detail: "Session not found.",
                        statusCode: 400,
                        instance: "/session/extension",
                        title: "Bad Request",
                        type: "/help"
                    );
                }
                var user = sessions.First().User;
                __DBContext.ClearExpriedSessionSocialUser(user.GetExpiredSessions(EXPIRY_TIME));
                __DBContext.SaveChanges();
                sessions = __DBContext.SessionSocialUsers
                            .Where(e => e.SessionToken == sessionToken)
                            .ToList();
                if (sessions.Count < 1) {
                    LogInformation($"Session has expired, session_token: { sessionToken.Substring(0, 15) }");
                    return Problem(
                        detail: "Session has expired.",
                        statusCode: 401,
                        instance: "/session/extension",
                        title: "Bad Request",
                        type: "/help"
                    );
                }
                user.SessionExtension(sessionToken, EXTENSION_TIME);
                __DBContext.SaveChanges();
                LogDebug($"Session extension success, session_token: { sessionToken.Substring(0, 15) }");
                return Ok( new JObject(){
                    { "status", 200 },
                    { "message", "success" },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.Message }");
                return Problem(
                    detail: "Internal Server error.",
                    statusCode: 500,
                    instance: "/session/extension",
                    title: "Internal Server Error",
                    type: "/report"
                );
            }
        }
    }
}
