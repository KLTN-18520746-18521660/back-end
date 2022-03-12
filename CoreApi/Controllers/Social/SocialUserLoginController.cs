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
using CoreApi.Services;

namespace CoreApi.Controllers.Social
{
    [ApiController]
    [Route("/login")]
    public class SocialUserLoginController : BaseController
    {
        private DBContext __DBContext;
        private BaseConfig __BaseConfig;
        /////////// CONFIG VALUE ///////////
        private int NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE;
        private int LOCK_TIME; // minute
        /////////////////////////////////////
        public SocialUserLoginController(
            DBContext _DBContext,
            BaseConfig _BaseConfig
        ) : base() {
            __DBContext = _DBContext;
            __BaseConfig = _BaseConfig;
            __ControllerName = "SocialUserLogin";
            LoadConfig();
        }
        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG, "number", out Error);
                LogWarning(Error);
                LOCK_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG, "lock", out Error);
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
        public IActionResult SocialUserLogin(Models.LoginModel model)
        {
            if (!LoadConfigSuccess) {
                return Problem(
                    detail: "Internal Server error.",
                    statusCode: 500,
                    instance: "/login",
                    title: "Internal Server Error",
                    type: "/report"
                );
            }
            try {
                // check input is email or not
                bool isEmail = CoreApi.Common.Utils.IsEmail(model.user_name);
                List<SocialUser> users;
                // Find user
                LogDebug($"Find user user_name: { model.user_name }, isEmail: { isEmail }");
                if (isEmail) {
                    users = __DBContext.SocialUsers
                            .Where<SocialUser>(e => e.Email == model.user_name
                                && e.StatusStr != BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus))
                            .ToList<SocialUser>();
                } else {
                    users = __DBContext.SocialUsers
                            .Where<SocialUser>(e => e.UserName == model.user_name
                                && e.StatusStr != BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus))
                            .ToList<SocialUser>();
                }

                if (users.Count < 1) {
                    LogDebug($"Not found user_name: { model.user_name }, isEmail: { isEmail }");
                    return Problem(
                        detail: "User not found or incorrect password.",
                        statusCode: 400,
                        instance: "/login",
                        title: "Bad Request",
                        type: "/help"
                    );
                }
                var user = users.First();
                if (user.Status == SocialUserStatus.Blocked) {
                    LogInformation($"User has been locked user_name: { model.user_name }, isEmail: { isEmail }");
                    return Problem(
                        detail: "You have been locked.",
                        statusCode: 423,
                        instance: "/login",
                        title: "Locked",
                        type: "/help"
                    );
                }
                if (PasswordEncryptor.EncryptPassword(model.password, user.Salt) != user.Password) {
                    LogDebug($"Incorrect password user_name: { model.user_name }, isEmail: { isEmail }");
                    HandleSocialUserLoginFail(user);
                    __DBContext.SaveChanges();
                    return Problem(
                        detail: "User not found or incorrect password.",
                        statusCode: 400,
                        instance: "/login",
                        title: "Bad Request",
                        type: "/help"
                    );
                }

                SessionSocialUser session = new(); 
                session.UserId = user.Id;
                session.Saved = model.remember;
                session.Data = model.data == null ? new JObject() : model.data;
                __DBContext.SessionSocialUsers.Add(session);
                HandleSocialUserLoginSuccess(user);

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
                    instance: "/login",
                    title: "Internal Server Error",
                    type: "/report"
                );
            }
        }
        [NonAction]
        public void HandleSocialUserLoginFail(SocialUser user) {
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

                    if (user.Status == SocialUserStatus.Blocked) {
                        long currentUnixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                        if (currentUnixTime - lastLoginFailure > LOCK_TIME * 60) {
                            user.Status = SocialUserStatus.Activated;
                            numberLoginFailure = 1;
                            lastLoginFailure = DateTimeOffset.Now.ToUnixTimeSeconds();
                        }
                    } else {
                        numberLoginFailure++;
                        lastLoginFailure = DateTimeOffset.Now.ToUnixTimeSeconds();
                    }

                    if (numberLoginFailure >= NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE) {
                        user.Status = (int) SocialUserStatus.Blocked;
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
        public void HandleSocialUserLoginSuccess(SocialUser user) {
            user.Settings.Remove("__login_config");
        }
    }
}
