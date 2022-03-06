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

namespace CoreApi.Controllers.Admin
{
    [ApiController]
    [Route("/admin/login")]
    public class AdminUserLoginController : BaseController
    {
        private DBContext __DBContext;
        private IBaseConfig __BaseConfig;
        /////////// CONFIG VALUE ///////////
        private int NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE;
        private int TIME_TO_SET_LOGIN_ATTEMPTS_TO_ZERO; // minute
        private int LOCK_TIME; // minute
        ////////////////////////////////////
        public AdminUserLoginController(
            DBContext _DBContext,
            IBaseConfig _BaseConfig
        ) : base() {
            __DBContext = _DBContext;
            __BaseConfig = _BaseConfig;
            __ControllerName = "AdminUserLogin";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG, "number", Error);
                LogWarning(Error);
                TIME_TO_SET_LOGIN_ATTEMPTS_TO_ZERO = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG, "time", Error);
                LogWarning(Error);
                LOCK_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG, "lock", Error);
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
        public IActionResult AdminUserLogin(Models.LoginModel model)
        {
            if (!LoadConfigSuccess) {
                return Problem(
                    detail: "Internal Server error.",
                    statusCode: 500,
                    instance: "/admin/login",
                    title: "Internal Server Error",
                    type: "/report"
                );
            }
            try {
                // check input is email or not
                bool isEmail = CoreApi.Common.Utils.isEmail(model.user_name);
                List<AdminUser> users;
                // Find user
                LogDebug($"Find user user_name: { model.user_name }, isEmail: { isEmail }");
                if (isEmail) {
                    users = __DBContext.AdminUsers
                            .Where<AdminUser>(e => e.Email == model.user_name
                                && e.StatusStr != BaseStatus.StatusToString(AdminUserStatus.Deleted, EntityStatus.AdminUserStatus))
                            .ToList<AdminUser>();
                } else {
                    users = __DBContext.AdminUsers
                            .Where<AdminUser>(e => e.UserName == model.user_name
                                && e.StatusStr != BaseStatus.StatusToString(AdminUserStatus.Deleted, EntityStatus.AdminUserStatus))
                            .ToList<AdminUser>();
                }

                if (users.Count < 1) {
                    LogDebug($"Not found user_name: { model.user_name }, isEmail: { isEmail }");
                    return Problem(
                        detail: "User not found or incorrect password.",
                        statusCode: 400,
                        instance: "/admin/login",
                        title: "Bad Request",
                        type: "/help"
                    );
                }

                var user = users.First();
                if (user.Status == AdminUserStatus.Blocked) {
                    LogInformation($"User has been locked user_name: { model.user_name }, isEmail: { isEmail }");
                    return Problem(
                        detail: "You have been locked.",
                        statusCode: 423,
                        instance: "/admin/login",
                        title: "Locked",
                        type: "/help"
                    );
                }
                if (PasswordEncryptor.EncryptPassword(model.password, user.Salt) != user.Password) {
                    LogInformation($"Incorrect password user_name: { model.user_name }, isEmail: { isEmail }");
                    HandleAdminUserLoginFail(user);
                    __DBContext.SaveChanges();
                    return Problem(
                        detail: "User not found or incorrect password.",
                        statusCode: 400,
                        instance: "/admin/login",
                        title: "Bad Request",
                        type: "/help"
                    );
                }

                SessionAdminUser session = new(); 
                session.UserId = user.Id;
                session.Saved = model.remember;
                session.Data = model.data == null ? new JObject() : model.data;
                __DBContext.SessionAdminUsers.Add(session);
                HandleAdminUserLoginSuccess(user);

                LogInformation($"User login success user_name: { model.user_name }, isEmail: { isEmail }");
                __DBContext.SaveChanges();
                return Ok( new JObject(){
                    { "status", 200 },
                    { "session_id", session.SessionToken },
                    { "user_id", user.Id },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.Message }");
                return Problem(
                    detail: "Internal Server error.",
                    statusCode: 500,
                    instance: "/admin/login",
                    title: "Internal Server Error",
                    type: "/report"
                );
            }
        }

        [NonAction]
        public void HandleAdminUserLoginFail(AdminUser user) {
            JObject config;
            if (!user.Settings.ContainsKey("__login_config")) {
                config = new JObject{
                    { "number", 1 },
                    { "last_login", DateTimeOffset.Now.ToUnixTimeSeconds()}
                };
            } else {
                try {
                    config = user.Settings.Value<JObject>("__login_config");
                    int numberLoginFailure = config.Value<int>("number");
                    long lastLoginFailure = config.Value<long>("last_login");

                    if (user.Status == AdminUserStatus.Blocked) {
                        long currentUnixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                        if (currentUnixTime - lastLoginFailure > LOCK_TIME * 60) {
                            user.Status = AdminUserStatus.Activated;
                            numberLoginFailure = 1;
                            lastLoginFailure = DateTimeOffset.Now.ToUnixTimeSeconds();
                        }
                    } else {
                        numberLoginFailure++;
                        lastLoginFailure = DateTimeOffset.Now.ToUnixTimeSeconds();
                    }

                    if (numberLoginFailure >= NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE) {
                        if (user.UserName != AdminUser.GetAdminUserName()) {
                            user.Status = (int) AdminUserStatus.Blocked;
                        }
                    }

                    config["number"] = numberLoginFailure;
                    config["last_login"] = lastLoginFailure;
                } catch (Exception) {
                    config = new JObject(){
                        { "number", 1 },
                        { "last_login", DateTimeOffset.Now.ToUnixTimeSeconds()}
                    };
                }
            }

            user.Settings["__login_config"] = config;
            return;
        }

        [NonAction]
        public void HandleAdminUserLoginSuccess(AdminUser user) {
            user.Settings.Remove("__login_config");
        }
    }
}
