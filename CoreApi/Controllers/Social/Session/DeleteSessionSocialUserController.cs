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

namespace CoreApi.Controllers.Social.Session
{
    [ApiController]
    [Route("/api/session")]
    public class DeleteSessionSocialUserController : BaseController
    {
        public DeleteSessionSocialUserController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        /// <summary>
        /// Delete social session
        /// </summary>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="__DeleteSessionToken"></param>
        /// <param name="SessionToken"></param>
        /// <returns><b>Message ok</b></returns>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header 'session_token'.
        /// - Need path param 'session_token' for delete.
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> return message <q>Success</q>.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Bad request params, header.</li>
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
        /// </ul>
        /// </response>
        /// 
        /// <response code="404">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Not found session to delete.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i> See server log for detail.</i>
        /// </response>
        [HttpDelete("{delete_session_token}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeleteSessionSocialUserSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> DeleteSession([FromServices] SessionSocialUserManagement           __SessionSocialUserManagement,
                                                       [FromRoute(Name = "delete_session_token")] string    __DeleteSessionToken,
                                                       [FromHeader(Name = "session_token")] string          SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SessionSocialUserManagement);
            #endregion
            try {
                #region Get session
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Compare with present session token
                AddLogParam("delete_session_token", __DeleteSessionToken);
                if (!CommonValidate.IsValidSessionToken(__DeleteSessionToken)) {
                    return Problem(400, "Invalid session token.");
                }
                if (__DeleteSessionToken == SessionToken) {
                    return Problem(400, "Not allow delete session. Try logout.");
                }
                #endregion

                #region Delete session
                var (DelSession, Error) = await __SessionSocialUserManagement.FindSession(__DeleteSessionToken);
                if (Error != ErrorCodes.NO_ERROR || DelSession.UserId != Session.UserId) {
                    return Problem(404, "Delete session not found.");
                }
                Error = await __SessionSocialUserManagement.RemoveSession(DelSession.SessionToken);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"DeleteSessionSocial Failed. ErrorCode: { Error }");
                }
                #endregion

                return Ok(200, "OK");
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }

        [HttpPost("removeall")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeleteSessionSocialUserSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> DeleteAllSession([FromServices] SessionSocialUserManagement        __SessionSocialUserManagement,
                                                          [FromHeader(Name = "session_token")] string       SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SessionSocialUserManagement);
            #endregion
            try {
                #region Get session
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Delete all session
                var Error = await __SessionSocialUserManagement.RemoveAllSession(Session.UserId, new string[]{ SessionToken });
                if (Error != ErrorCodes.NO_ERROR && Error != ErrorCodes.NO_CHANGE_DETECTED) {
                    throw new Exception($"RemoveAllSession Failed. ErrorCode: { Error }");
                }
                #endregion

                return Ok(200, Error == ErrorCodes.NO_ERROR ? "OK" : "No change detected");
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
