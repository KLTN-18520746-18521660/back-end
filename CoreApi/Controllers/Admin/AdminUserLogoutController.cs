using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using Common;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace CoreApi.Controllers.Admin
{
    [ApiController]
    [Route("/admin/logout")]
    public class AdminUserLogoutController : BaseController
    {
        #region Services
        private BaseConfig __BaseConfig;
        private SessionAdminUserManagement __SessionAdminUserManagement;
        #endregion

        #region Config Value
        private int EXPIRY_TIME; // minute
        #endregion

        public AdminUserLogoutController(
            BaseConfig _BaseConfig,
            SessionAdminUserManagement _SessionAdminUserManagement
        ) : base() {
            __BaseConfig = _BaseConfig;
            __SessionAdminUserManagement = _SessionAdminUserManagement;
            __ControllerName = "AdminUserLogout";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                EXPIRY_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, "expiry_time", out Error);
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
        /// Admin user logout
        /// </summary>
        /// <returns><b>Return message ok</b></returns>
        ///
        /// <remarks>
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> return message <q>Success.</q>.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Session not found.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="403">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Missing header session_token.</li>
        /// <li>Header session_token is invalid.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminUserLogoutSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public IActionResult AdminUserLogout()
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

                #region Find session token
                SessionAdminUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;

                if (!__SessionAdminUserManagement.FindSession(sessionToken, out session, out error)) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(400, "Session not found.");
                    }
                    throw new Exception("Internal Server Error. FindSessionAdminUser failed.");
                }
                #endregion

                #region Remove session and clear expried session
                var user = session.User;
                if (!__SessionAdminUserManagement.RemoveSession(session, out error)) {
                    throw new Exception("Internal Server Error. RemoveSessionAdminUser failed.");
                }
                if (!__SessionAdminUserManagement.ClearExpiredSession(user, EXPIRY_TIME, out error)) {
                    throw new Exception("Internal Server Error. ClearExpiredSessionAdminUser failed.");
                }
                #endregion

                LogInformation($"Logout success, user_name: { user.UserName }, session_token: { sessionToken.Substring(0, 15) }");
                return Ok( new JObject(){
                    { "status", 200 },
                    { "message", "Success." },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
