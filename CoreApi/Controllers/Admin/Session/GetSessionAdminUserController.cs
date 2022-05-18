using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Admin.Session
{
    [ApiController]
    [Route("/api/admin/session")]
    public class GetSessionAdminUserController : BaseController
    {
        public GetSessionAdminUserController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        /// <summary>
        /// Get all admin session of user
        /// </summary>
        /// <returns><b>All admin session of user</b></returns>
        /// <param name="__SessionAdminUserManagement"></param>
        /// <param name="SessionToken"></param>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header 'session_token'.
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> All admin session of user.
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
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAllSessionAdminUserSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetSessions([FromServices] SessionAdminUserManagement      __SessionAdminUserManagement,
                                                     [FromHeader(Name = "session_token")] string    SessionToken)
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

                #region Get all sessions
                var (AllSessionOfUser, Error) = await __SessionAdminUserManagement.GetAllSessionOfUser(Session.UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetAllAdminUserSessions Failed. ErrorCode: { Error }");
                }

                var RawRet = new List<JObject>();
                AllSessionOfUser.ForEach(e => RawRet.Add(e.GetJsonObject()));
                var Ret = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(RawRet));
                #endregion

                return Ok(200, "OK", new JObject(){
                    { "sessions", Ret.ToString() },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }

        /// <summary>
        /// Get admin session of user by id
        /// </summary>
        /// <param name="__SessionAdminUserManagement"></param>
        /// <param name="__GetSessionToken"></param>
        /// <param name="SessionToken"></param>
        /// <returns><b>Admin session of user</b></returns>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header 'session_token'.
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> Admin session of user.
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
        /// <response code="404">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Session not found.</li>
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
        [HttpGet("{get_session_token}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetSessionAdminUserSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetSessionById([FromServices] SessionAdminUserManagement           __SessionAdminUserManagement,
                                                        [FromRoute(Name = "get_session_token")] string      __GetSessionToken,
                                                        [FromHeader(Name = "session_token_admin")] string   SessionToken)
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

                #region Get session for return
                AddLogParam("get_session_token", __GetSessionToken);
                var (Ret, Error) = await __SessionAdminUserManagement.FindSession(__GetSessionToken);
                if (Error != ErrorCodes.NO_ERROR || Ret.UserId != Session.UserId) {
                    return Problem(404, "Session not found.");
                }
                #endregion

                return Ok(200, "OK", new JObject(){
                    { "session", Ret.GetJsonObject() },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
