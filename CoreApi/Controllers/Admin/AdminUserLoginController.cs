using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Admin
{
    [ApiController]
    [Route("/admin/login")]
    public class AdminUserLoginController : BaseController
    {
        #region Config Values
        private int NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE;
        private int LOCK_TIME; // minute
        #endregion

        public AdminUserLoginController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "AdminUserLogin";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                (NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG, SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE);
                (LOCK_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG, SUB_CONFIG_KEY.LOCK_TIME);
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
        /// <param name="__AdminUserManagement"></param>
        /// <param name="__SessionAdminUserManagement"></param>
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
        public async Task<IActionResult> AdminUserLogin([FromServices] AdminUserManagement __AdminUserManagement,
                                                        [FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                        [FromBody] Models.LoginModel model)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __AdminUserManagement.SetTraceId(TraceId);
            __SessionAdminUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Find User
                bool isEmail = CommonValidate.IsEmail(model.user_name);
                LogDebug($"Find user user_name: { model.user_name }, isEmail: { isEmail }");
                ErrorCodes error = ErrorCodes.NO_ERROR;
                AdminUser user = null;
                (user, error) = await __AdminUserManagement.FindUser(model.user_name, isEmail);

                if (error != ErrorCodes.NO_ERROR) {
                    LogDebug($"Not found user_name: { model.user_name }, isEmail: { isEmail }");
                    return Problem(400, "User not found or incorrect password.");
                }
                #endregion

                #region Check user is lock or not
                if (user.Status == AdminUserStatus.Blocked) {
                    LogWarning($"User has been locked user_name: { model.user_name }, isEmail: { isEmail }");
                    return Problem(423, "You have been locked.");
                }
                #endregion

                #region Compare password
                if (PasswordEncryptor.EncryptPassword(model.password, user.Salt) != user.Password) {
                    LogInformation($"Incorrect password user_name: { model.user_name }, isEmail: { isEmail }");
                    error = await __AdminUserManagement.HandleLoginFail(user.Id, LOCK_TIME, NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE);
                    if (error != ErrorCodes.NO_ERROR) {
                        throw new Exception($"Handle AdminUseLoginFail failed. ErrorCode: { error }");
                    }
                    return Problem(400, "User not found or incorrect password.");
                }
                #endregion

                #region Create session
                SessionAdminUser session = null;
                var data = model.data == null ? new JObject() : model.data;
                (session, error) = await __SessionAdminUserManagement.NewSession(user.Id, model.remember, data);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"CreateNewAdminSession Failed. ErrorCode: { error }");
                }

                error = await __AdminUserManagement.HandleLoginSuccess(user.Id);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"Handle AdminUserLoginSuccess failed. ErrorCode: { error }");
                }
                #endregion

                LogInformation($"User login success user_name: { model.user_name }, isEmail: { isEmail }");
                return Ok(200, "OK", new JObject(){
                    { "session_id", session.SessionToken },
                    { "user_id", user.Id },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}