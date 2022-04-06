using Common;
using CoreApi.Common;
using CoreApi.Services;
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
    [Route("/admin/logout")]
    public class AdminUserLogoutController : BaseController
    {
        #region Config Values
        private int EXPIRY_TIME; // minute
        #endregion

        public AdminUserLogoutController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "AdminUserLogout";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
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
        /// Admin user logout
        /// </summary>
        /// <returns><b>Return message ok</b></returns>
        /// <param name="__SessionAdminUserManagement"></param>
        /// <param name="session_token"></param>
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
        public async Task<IActionResult> AdminUserLogout([FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                         [FromHeader] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionAdminUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session token
                if (session_token == null) {
                    LogDebug($"Missing header authorization.");
                    return Problem(403, "Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Find session token
                SessionAdminUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionAdminUserManagement.FindSession(session_token);

                if (error != ErrorCodes.NO_ERROR) {
                    LogDebug($"Session not found, session_token: { session_token.Substring(0, 15) }");
                    return Problem(400, "Session not found.");
                }
                #endregion

                #region Remove session and clear expried session
                var user = session.User;
                error = await __SessionAdminUserManagement.RemoveSession(session.SessionToken);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"RemoveSessionAdminUser failed. ErrorCode: { error }");
                }
                error = await __SessionAdminUserManagement.ClearExpiredSession(user.GetExpiredSessions(EXPIRY_TIME));
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"ClearExpiredSessionAdminUser failed. ErrorCode: { error }");
                }
                #endregion

                LogInformation($"Logout success, user_name: { user.UserName }, session_token: { session_token.Substring(0, 15) }");
                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
