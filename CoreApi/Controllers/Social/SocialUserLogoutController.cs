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

using System.Text;
// using System.Data.Entity;
using System.Diagnostics;
// using System.Text.Json;
using CoreApi.Services;

namespace CoreApi.Controllers.Social
{
    [ApiController]
    [Route("/logout")]
    public class SocialUserLogoutController : BaseController
    {
        private DBContext __DBContext;
        private BaseConfig __BaseConfig;
        /////////// CONFIG VALUE ///////////
        private int EXPIRY_TIME; // minute
        ////////////////////////////////////
        public SocialUserLogoutController(
            DBContext _DBContext,
            BaseConfig _BaseConfig
        ) : base() {
            __DBContext = _DBContext;
            __BaseConfig = _BaseConfig;
            __ControllerName = "SocialUserLogout";
            LoadConfig();
        }
        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                EXPIRY_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, "expiry_time", out Error);
                LogWarning(Error);
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
        [HttpPost]
        public IActionResult SocialUserLogout()
        {
            if (!LoadConfigSuccess) {
                return Problem(
                    detail: "Internal Server error.",
                    statusCode: 500,
                    instance: "/logout",
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
                        instance: "/logout",
                        title: "Bad Request",
                        type: "/help"
                    );
                }

                string sessionToken = Request.Headers[HEADER_KEYS.API_KEY];
                if (!CoreApi.Common.Utils.IsValidSessionToken(sessionToken)) {
                    return Problem(
                        detail: "Invalid header authorization.",
                        statusCode: 403,
                        instance: "/getuser",
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
                        instance: "/logout",
                        title: "Bad Request",
                        type: "/help"
                    );
                }
                var userName = sessions.First().User.UserName;
                var expiredSessions = sessions.First().User.GetExpiredSessions(EXPIRY_TIME);

                __DBContext.SessionSocialUsers.Remove(sessions.First());
                __DBContext.SaveChanges();
                __DBContext.ClearExpriedSessionSocialUser(expiredSessions);
                __DBContext.SaveChanges();
                LogInformation($"Logout success, user_name: { userName }");
                return Ok( new JObject(){
                    { "status", 200 },
                    { "message", "success" },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.Message }");
                return Problem(
                    detail: "Internal Server error.",
                    statusCode: 500,
                    instance: "/logout",
                    title: "Internal Server Error",
                    type: "/report"
                );
            }
        }
    }
}
