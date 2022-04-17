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

namespace CoreApi.Controllers.Social
{
    [ApiController]
    [Route("/logout")]
    public class SocialUserLogoutController : BaseController
    {
        #region Config Values
        private int EXPIRY_TIME; // minute
        #endregion
        public SocialUserLogoutController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "SocialUserLogout";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = string.Empty;
            try {
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
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
        /// Social user logout
        /// </summary>
        /// <returns><b>Return message ok</b></returns>
        /// <param name="__SessionSocialUserManagement"></param>
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SocialUserLogoutSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> SocialUserLogout([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                          [FromHeader] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session token
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                    return Problem(403, "Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Find session token
                SessionSocialUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionSocialUserManagement.FindSession(session_token);

                if (error != ErrorCodes.NO_ERROR) {
                    LogDebug($"Session not found, session_token: { session_token.Substring(0, 15) }");
                    return Problem(401, "Session not found.");
                }
                #endregion

                #region Remove session and clear expried session
                var user = session.User;
                error = await __SessionSocialUserManagement.RemoveSession(session.SessionToken);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"RemoveSessionSocialUser failed. ErrorCode: { error }");
                }
                error = await __SessionSocialUserManagement.ClearExpiredSession(user.GetExpiredSessions(EXPIRY_TIME));
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"ClearExpiredSessionSocialUser failed. ErrorCode: { error }");
                }
                #endregion

                LogInformation($"Logout success, user_name: { user.UserName }, session_token: { session_token.Substring(0, 15) }");

                #region cookie header
                CookieOptions option = new CookieOptions();
                option.Expires = new DateTime(1970, 1, 1, 0, 0, 0);
                option.Path = "/";
                option.SameSite = SameSiteMode.Strict;

                Response.Cookies.Append("session_token", session.SessionToken, option);
                #endregion

                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
