using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Common;
using Microsoft.AspNetCore.Http;

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
                StringBuilder msg = new StringBuilder(e.ToString());
                if (Error != e.Message && Error != "") {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value failed, message: { msg }");
            }
        }

        /// <summary>
        /// Admin user login
        /// </summary>
        /// <param name="model"></param>
        /// <returns><b>Return session_id</b></returns>
        ///
        /// <remarks>
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> return 'session_id' and 'user_id'.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Bad request body.</li>
        /// <li>User not found or incorrect password.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="423">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>User have been locked.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminUserLoginSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public IActionResult AdminUserLogin(Models.LoginModel model)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            try {
                #region Find User
                ErrorCodes error = ErrorCodes.NO_ERROR;
                bool isEmail = Utils.IsEmail(model.user_name);
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
                LogError($"Unhandle exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
