using Common;
using CoreApi.Common.Base;
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
    [Route("/api/logout")]
    public class SocialUserLogoutController : BaseController
    {
        public SocialUserLogoutController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        /// <summary>
        /// Social user logout
        /// </summary>
        /// <returns><b>Return message ok</b></returns>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="SessionToken"></param>
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
        public async Task<IActionResult> SocialUserLogout([FromServices] SessionSocialUserManagement    __SessionSocialUserManagement,
                                                          [FromHeader(Name = "session_token")] string   SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SessionSocialUserManagement);
            #endregion
            try {
                #region Get config values
                var ExpiryTime      = GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                #endregion

                #region Validate session token
                SessionToken = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                if (SessionToken == default) {
                    return Problem(401, RESPONSE_MESSAGES.MISSING_HEADER_AUTHORIZE, default, default, LOG_LEVEL.DEBUG);
                }

                if (!CommonValidate.IsValidSessionToken(SessionToken)) {
                    return Problem(401, RESPONSE_MESSAGES.INVALID_HEADER_AUTHORIZE, default, default, LOG_LEVEL.DEBUG);
                }
                #endregion

                #region Find session token
                var (Session, Error) = await __SessionSocialUserManagement.FindSession(SessionToken);

                if (Error != ErrorCodes.NO_ERROR) {
                    return Problem(401, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "session" }, default, LOG_LEVEL.DEBUG);
                }
                #endregion

                #region Remove session and clear expried session
                Error = await __SessionSocialUserManagement.RemoveSession(Session.SessionToken);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"RemoveSession failed. ErrorCode: { Error }");
                }
                Error = await __SessionSocialUserManagement.ClearExpiredSession(Session.User.GetExpiredSessions(ExpiryTime));
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"ClearExpiredSession failed. ErrorCode: { Error }");
                }
                #endregion

                #region Set cookie header to remove session token
                Response.Cookies.Append(SessionTokenHeaderKey, string.Empty, GetCookieOptionsForDelete());
                #endregion

                return Ok(200, RESPONSE_MESSAGES.OK);
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
