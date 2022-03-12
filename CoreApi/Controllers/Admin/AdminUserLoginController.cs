using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace CoreApi.Controllers.Admin
{
    [ApiController]
    [Route("/admin/login")]
    public class AdminUserLoginController : BaseController
    {
        #region Services
        private BaseConfig __BaseConfig;
        private AdminUserManagement __AdminUserManagement;
        private SessionAdminUserManagement __SessionAdminUserManagement;
        #endregion

        #region Config Value
        private int NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE;
        private int LOCK_TIME; // minute
        #endregion

        public AdminUserLoginController(
            BaseConfig _BaseConfig,
            AdminUserManagement _AdminUserManagement,
            SessionAdminUserManagement _SessionAdminUserManagement
        ) : base() {
            __BaseConfig = _BaseConfig;
            __AdminUserManagement = _AdminUserManagement;
            __SessionAdminUserManagement = _SessionAdminUserManagement;
            __ControllerName = "AdminUserLogin";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG, "number", out Error);
                LOCK_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG, "lock", out Error);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.Message);
                if (Error != e.Message && Error != "") {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value failed, message: { msg }");
            }
        }

        [HttpPost("")]
        // [ApiExplorerSettings(GroupName = "admin")]
        public IActionResult AdminUserLogin(Models.LoginModel model)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            try {
                #region Find User
                ErrorCodes error = ErrorCodes.NO_ERROR;
                bool isEmail = CoreApi.Common.Utils.IsEmail(model.user_name);
                LogDebug($"Find user user_name: { model.user_name }, isEmail: { isEmail }");
                AdminUser user = null;
                bool found = __AdminUserManagement.FindUser(model.user_name, isEmail, out user, out error);

                if (!found) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Not found user_name: { model.user_name }, isEmail: { isEmail }");
                        return Problem(400, "User not found or incorrect password.");
                    }
                    throw new Exception("Internal Server Error. Find AdminUser failed.");
                }
                #endregion

                #region Check user is lock or not
                if (user.Status == AdminUserStatus.Blocked) {
                    LogInformation($"User has been locked user_name: { model.user_name }, isEmail: { isEmail }");
                    return Problem(423, "You have been locked.");
                }
                #endregion

                #region Compare password
                if (PasswordEncryptor.EncryptPassword(model.password, user.Salt) != user.Password) {
                    LogInformation($"Incorrect password user_name: { model.user_name }, isEmail: { isEmail }");
                    __AdminUserManagement.HandleLoginFail(user, LOCK_TIME, NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE, out error);
                    if (error != ErrorCodes.NO_ERROR) {
                        throw new Exception("Internal Server Error. Handle AdminUseLoginFail failed.");
                    }
                    return Problem(400, "User not found or incorrect password.");
                }
                #endregion

                #region Create session
                SessionAdminUser session = null;
                var data = model.data == null ? new JObject() : model.data;
                if (!__SessionAdminUserManagement.NewSession(user.Id, model.remember, data, out session, out error)) {
                    throw new Exception("Internal Server Error. CreateNewAdminSession Failed.");
                }

                __AdminUserManagement.HandleLoginSuccess(user, out error);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception("Internal Server Error. Handle AdminUserLoginSuccess failed.");
                }
                #endregion

                LogInformation($"User login success user_name: { model.user_name }, isEmail: { isEmail }");
                return Ok( new JObject(){
                    { "status", 200 },
                    { "session_id", session.SessionToken },
                    { "user_id", user.Id },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.Message }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
