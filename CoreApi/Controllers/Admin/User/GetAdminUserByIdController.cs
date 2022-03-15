using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Common;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Admin.User
{
    [ApiController]
    [Route("/admin/user")]
    public class GetAdminUserByIdController : BaseController
    {
        #region Services
        private AdminUserManagement __AdminUserManagement;
        private SessionAdminUserManagement __SessionAdminUserManagement;
        private BaseConfig __BaseConfig;
        #endregion

        #region Config Value
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetAdminUserByIdController(
            AdminUserManagement _AdminUserManagement,
            SessionAdminUserManagement _SessionAdminUserManagement,
            BaseConfig _BaseConfig
        ) : base() {
            __AdminUserManagement = _AdminUserManagement;
            __SessionAdminUserManagement = _SessionAdminUserManagement;
            __BaseConfig = _BaseConfig;
            __ControllerName = "GetAdminUserById";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
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
        /// Get admin user info by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns><b>Get admin user info by id</b></returns>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header 'session_token'.
        /// - User have read permission of 'admin_user'.
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> return admin user info.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Bad request body.</li>
        /// <li>Field 'user_name' or 'email' has been used.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="401">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="403">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Missing header session_token.</li>
        /// <li>Header session_token is invalid.</li>
        /// <li>User doesn't have permission to get admin user.</li>
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
        [HttpGet("/admin/user/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAdminUserByIdSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetSocialUserByApiKeyAsync(Guid id)
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

                if (!Utils.IsValidSessionToken(sessionToken)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionAdminUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionAdminUserManagement.FindSessionForUse(sessionToken, EXPIRY_TIME, EXTENSION_TIME);

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(400, "Session not found.");
                    }
                    if (error == ErrorCodes.SESSION_HAS_EXPIRED) {
                        LogInformation($"Session has expired, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(401, "Session has expired.");
                    }
                    if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogInformation($"User has been locked, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(423, "You have been locked.");
                    }
                    throw new Exception($"FindSessionForUse Failed. ErrorCode: { error }");
                }
                #endregion

                #region Check Permission
                var user = session.User;
                if (__AdminUserManagement.HaveReadPermission(user.Rights, ADMIN_RIGHTS.ADMIN_USER) == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    LogInformation($"User doesn't have permission for get admin user, user_name: { user.UserName }");
                    return Problem(403, "User doesn't have permission for get admin user.");
                }
                #endregion

                #region Get Admin user info by id
                AdminUser retUser = null;
                (retUser, error) = await __AdminUserManagement.FindUserById(id);
                if (error != ErrorCodes.NO_ERROR) {
                    LogDebug($"User not found by id: { id }");
                    return Problem(404, "User not found.");
                }
                #endregion

                LogInformation($"Get info user by id success, user_name: { user.UserName }, id: { id }");
                return Ok( new JObject(){
                    { "status", 200 },
                    { "user", retUser.GetJsonObject() },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
