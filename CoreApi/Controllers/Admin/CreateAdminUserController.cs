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
using DatabaseAccess.Context.ParserModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CoreApi.Common;
// using System.Data.Entity;
using System.Diagnostics;
using CoreApi.Common.Interface;
using System.Text;
// using System.Text.Json;

namespace CoreApi.Controllers.Admin
{
    [ApiController]
    [Route("/admin/user")]
    public class CreateAdminUserController : BaseController
    {
        private DBContext __DBContext;
        private IBaseConfig __BaseConfig;
        /////////// CONFIG VALUE ///////////
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minutes
        ////////////////////////////////////
        public CreateAdminUserController(
            DBContext _DBContext,
            IBaseConfig _BaseConfig
        ) : base() {
            __DBContext = _DBContext;
            __BaseConfig = _BaseConfig;
            __ControllerName = "CreateAdminUser";
            LoadConfig();
        }
        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                EXTENSION_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, "extension_time", Error);
                LogWarning(Error);
                EXPIRY_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, "expiry_time", Error);
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
        public IActionResult CreateAdminUser(ParserAdminUser parser)
        {
            if (!LoadConfigSuccess) {
                return Problem(
                    detail: "Internal Server error.",
                    statusCode: 500,
                    instance: "/admin/user",
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
                        instance: "/admin/user",
                        title: "Bad Request",
                        type: "/help"
                    );
                }
                AdminUser newUser = new AdminUser();
                string Error = "";
                if (!newUser.Parse(parser, out Error)) {
                    LogInformation(Error);
                    return Problem(
                        detail: "Bad body data.",
                        statusCode: 400,
                        instance: "/admin/user",
                        title: "Bad request",
                        type: "/help"
                    );
                }

                string sessionToken = Request.Headers[HEADER_KEYS.API_KEY];
                if (!CoreApi.Common.Utils.IsValidSessionToken(sessionToken)) {
                    return Problem(
                        detail: "Invalid header authorization.",
                        statusCode: 403,
                        instance: "/admin/user",
                        title: "Bad Request",
                        type: "/help"
                    );
                }
                var sessions = __DBContext.SessionAdminUsers
                                .Where(e => e.SessionToken == sessionToken)
                                .ToList();

                if (sessions.Count < 1) {
                    LogDebug($"Session not found, session_token: { sessionToken.Substring(0, 15) }");
                    return Problem(
                        detail: "Session not found.",
                        statusCode: 400,
                        instance: "/admin/user",
                        title: "Bad Request",
                        type: "/help"
                    );
                }
                var user = sessions.First().User;
                __DBContext.ClearExpriedSessionAdminUser(user.GetExpiredSessions(EXPIRY_TIME));
                __DBContext.SaveChanges();
                sessions = __DBContext.SessionAdminUsers
                            .Where(e => e.SessionToken == sessionToken)
                            .ToList();
                if (sessions.Count < 1) {
                    LogInformation($"Session has expired, session_token: { sessionToken.Substring(0, 15) }");
                    return Problem(
                        detail: "Session has expired.",
                        statusCode: 401,
                        instance: "/admin/user",
                        title: "Bad Request",
                        type: "/help"
                    );
                }
                user.SessionExtension(sessionToken, EXTENSION_TIME);
                __DBContext.SaveChanges();

                if (!HavePermissionForCreateAdminUser(user)) {
                    LogInformation($"User not have permission for create admin user, user_name: { user.UserName }");
                    return Problem(
                        detail: "User not have permission for create admin user.",
                        statusCode: 403,
                        instance: "/admin/user",
                        title: "Bad Request",
                        type: "/help"
                    );
                }
                // Check user name
                bool userNameHaveUsed = __DBContext.AdminUsers
                        .Count(e => 
                                e.UserName == newUser.UserName &&
                                e.StatusStr != BaseStatus.StatusToString(AdminUserStatus.Deleted, EntityStatus.AdminUserStatus)
                        ) > 0;
                if (userNameHaveUsed) {
                    LogDebug($"UserName have been used, user_name: { user.UserName }");
                    return Problem(
                        detail: "UserName have been used.",
                        statusCode: 400,
                        instance: "/admin/user",
                        title: "Bad request",
                        type: "/help"
                    );
                }
                // check email user
                bool userEmailHaveUsed = __DBContext.AdminUsers
                        .Count(e => 
                                e.Email == newUser.Email &&
                                e.StatusStr != BaseStatus.StatusToString(AdminUserStatus.Deleted, EntityStatus.AdminUserStatus)
                        ) > 0;
                if (userEmailHaveUsed) {
                    LogDebug($"Email have been used, user_name: { user.Email }");
                    return Problem(
                        detail: "Email have been used.",
                        statusCode: 400,
                        instance: "/admin/user",
                        title: "Bad request",
                        type: "/help"
                    );
                }


                // add user
                __DBContext.AdminUsers.Add(newUser);
                // save changes
                __DBContext.SaveChanges();
                LogInformation($"Create new admin user success, user_name: { newUser.UserName }");
                return Ok(new JObject(){
                    { "status", 200 },
                    { "message", "success" },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.Message }");
                return Problem(
                    detail: "Internal Server error.",
                    statusCode: 500,
                    instance: "/admin/user",
                    title: "Internal Server Error",
                    type: "/report"
                );
            }
        }
        [NonAction]
        public bool HavePermissionForCreateAdminUser(AdminUser user)
        {
            var rights = user.Rights;
            if (rights.ContainsKey("admin_user")) {
                var right = rights["admin_user"];
                if (right["read"] != null && right["write"] != null &&
                    ((bool)right["read"]) == true && ((bool)right["write"]) == true) {
                    return true;
                }
            }
            return false;
        }
    }
}
