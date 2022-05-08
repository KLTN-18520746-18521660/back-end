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
    [Route("/api/admin/logout")]
    public class AdminUserLogoutController : BaseController
    {
        #region Config Values
        private int EXPIRY_TIME; // minute
        private int EXTENSION_TIME; // minute
        #endregion

        public AdminUserLogoutController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "AdminUserLogout";
            __IsAdminController = true;
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = string.Empty;
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.ToString());
                if (Error != e.Message && Error != string.Empty) {
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
        /// <response code="401">
        /// <b>Error case <i>(Server auto send response with will clear cookie 'session_token_admin')</i>, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
        /// <li>Session not found.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminUserLogoutSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> AdminUserLogout([FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                         [FromHeader(Name = "session_token_admin")] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionAdminUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                var (__session, errRet) = await GetSessionToken(__SessionAdminUserManagement, EXPIRY_TIME, EXTENSION_TIME, session_token);
                if (errRet != default) {
                    return errRet;
                }
                if (__session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var session = __session as SessionAdminUser;
                #endregion

                #region Remove session and clear expried session
                var error = ErrorCodes.NO_ERROR;
                error = await __SessionAdminUserManagement.RemoveSession(session.SessionToken);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"RemoveSessionAdminUser failed. ErrorCode: { error }");
                }
                error = await __SessionAdminUserManagement.ClearExpiredSession(session.User.GetExpiredSessions(EXPIRY_TIME));
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"ClearExpiredSessionAdminUser failed. ErrorCode: { error }");
                }
                #endregion

                LogInformation($"Logout success, user_name: { session.User.UserName }, { SessionTokenHeaderKey }: { session_token.Substring(0, 15) }");

                #region set cookie header to remove session_token_admin
                CookieOptions option = new CookieOptions();
                option.Expires = new DateTime(1970, 1, 1, 0, 0, 0);
                option.Path = "/";
                option.SameSite = SameSiteMode.Strict;

                Response.Cookies.Append("session_token_admin", string.Empty, option);
                #endregion

                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
