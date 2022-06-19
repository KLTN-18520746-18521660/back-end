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

namespace CoreApi.Controllers.Admin.Session
{
    [ApiController]
    [Route("/api/admin/session")]
    public class DeleteSessionAdminUserController : BaseController
    {
        public DeleteSessionAdminUserController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        /// <summary>
        /// Delete admin session
        /// </summary>
        /// <param name="__SessionAdminUserManagement"></param>
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
        /// <b>Error case <i>(Server auto send response with will clear cookie 'session_token_admin')</i>, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
        /// <li>Session not found.</li>
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
        [HttpDelete("{delete_session_token}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeleteSessionAdminUserSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> DeleteSession([FromServices] SessionAdminUserManagement            __SessionAdminUserManagement,
                                                       [FromRoute(Name = "delete_session_token")] string    __DeleteSessionToken,
                                                       [FromHeader(Name = "session_token_admin")] string    SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SessionAdminUserManagement);
            #endregion
            try {
                #region Get session
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionAdminUserManagement, SessionToken);
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session             = __Session as SessionAdminUser;
                #endregion

                #region Compare with present session token
                AddLogParam("delete_session_token", __DeleteSessionToken);
                if (!CommonValidate.IsValidSessionToken(__DeleteSessionToken)) {
                    return Problem(400, RESPONSE_MESSAGES.INVALID_SESSION_TOKEN);
                }
                if (__DeleteSessionToken == SessionToken) {
                    return Problem(400, RESPONSE_MESSAGES.NOT_ALLOW_TO_DO, new string[]{ "delete session" });
                }
                #endregion

                #region Delete session
                var (DelSession, Error) = await __SessionAdminUserManagement.FindSession(__DeleteSessionToken);
                if (Error != ErrorCodes.NO_ERROR || DelSession.UserId != Session.UserId) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "delete session" });
                }
                Error = await __SessionAdminUserManagement.RemoveSession(DelSession.SessionToken);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"RemoveSession failed. ErrorCode: { Error }");
                }
                #endregion

                return Ok(200, RESPONSE_MESSAGES.OK);
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }

        [HttpPost("removeall")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeleteSessionAdminUserSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> DeleteAllSession([FromServices] SessionAdminUserManagement     __SessionAdminUserManagement,
                                                          [FromHeader(Name = "session_token")] string   SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SessionAdminUserManagement);
            #endregion
            try {
                #region Get session
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionAdminUserManagement, SessionToken);
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session             = __Session as SessionAdminUser;
                #endregion

                #region Delete all session
                var Error = await __SessionAdminUserManagement.RemoveAllSession(Session.UserId, new string[]{ SessionToken });
                if (Error != ErrorCodes.NO_ERROR && Error != ErrorCodes.NO_CHANGE_DETECTED) {
                    throw new Exception($"RemoveAllSession failed. ErrorCode: { Error }");
                }
                #endregion

                return Ok(200, Error == ErrorCodes.NO_ERROR ? RESPONSE_MESSAGES.OK : RESPONSE_MESSAGES.NO_CHANGES_DETECTED);
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
