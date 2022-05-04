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

namespace CoreApi.Controllers.Admin.Session
{
    [ApiController]
    [Route("/api/admin/session")]
    public class DeleteSessionAdminUserController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minutes
        #endregion

        public DeleteSessionAdminUserController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "DeleteSessionAdminUser";
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
        /// Delete admin session
        /// </summary>
        /// <param name="__SessionAdminUserManagement"></param>
        /// <param name="session_token"></param>
        /// <param name="delete_session_token"></param>
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
        public async Task<IActionResult> DeleteSession([FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                       [FromHeader(Name = "session_token_admin")] string session_token,
                                                       [FromRoute] string delete_session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionAdminUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session token
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                    return Problem(401, "Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    LogDebug($"Invalid header authorization.");
                    return Problem(401, "Invalid header authorization.");
                }
                #endregion

                #region Compare with present session token
                if (!CommonValidate.IsValidSessionToken(delete_session_token)) {
                    LogWarning($"Invalid session_token delete.");
                    return Problem(400, "Invalid session token.");
                }
                if (delete_session_token == session_token) {
                    LogWarning($"Not allow delete session, session_token same with seesion_token delete.");
                    return Problem(400, "Not allow delete session. Try logout.");
                }
                #endregion

                #region Find session for use
                SessionAdminUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionAdminUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogWarning($"Session not found, session_token: { session_token.Substring(0, 15) }");
                        return Problem(401, "Session not found.");
                    }
                    if (error == ErrorCodes.SESSION_HAS_EXPIRED) {
                        LogWarning($"Session has expired, session_token: { session_token.Substring(0, 15) }");
                        return Problem(401, "Session has expired.");
                    }
                    if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogWarning($"User has been locked, session_token: { session_token.Substring(0, 15) }");
                        return Problem(423, "You have been locked.");
                    }
                    throw new Exception($"FindSessionAdminForUse Failed. ErrorCode: { error }");
                }
                #endregion

                #region Delete session
                SessionAdminUser delSession = default;
                (delSession, error) = await __SessionAdminUserManagement.FindSession(delete_session_token);
                if (error != ErrorCodes.NO_ERROR || delSession.UserId != session.UserId) {
                    LogWarning($"Delete session not found, session_token: { delete_session_token.Substring(0, 15) }");
                    return Problem(404, "Delete session not found.");
                }
                error = await __SessionAdminUserManagement.RemoveSession(delSession.SessionToken);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"DeleteSessionAdmin Failed. ErrorCode: { error }");
                }
                #endregion

                LogInformation($"Delete session success, session_token: { delete_session_token.Substring(0, 15) }");
                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
