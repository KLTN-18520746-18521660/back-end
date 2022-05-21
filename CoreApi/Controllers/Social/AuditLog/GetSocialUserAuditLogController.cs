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

namespace CoreApi.Controllers.Social.AuditLog
{
    [ApiController]
    [Route("/api/auditlog")]
    public class GetSocialUserAuditLogController : BaseController
    {
        protected string[] AllowActions = new string[]{
            "comment",
            "post",
            "user",
        };

        public GetSocialUserAuditLogController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        /// <summary>
        /// Get social user auditlog
        /// </summary>
        /// <param name="__SocialUserManagement"></param>
        /// <param name="__SocialUserAuditLogManagement"></param>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="SessionToken"></param>
        /// <param name="Type"></param>
        /// <param name="Key"></param>
        /// <param name="Start"></param>
        /// <param name="Size"></param>
        /// <param name="SearchTerm"></param>
        /// <returns><b>List social user auditlog</b></returns>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header or cookie 'session_token'.
        /// - User have read permission of 'log'.
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> List auditlogs order by created_date desc.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Invalid params start, size</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="401">
        /// <b>Error case <i>(Server auto send response with will clear cookie 'session_token')</i>, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
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
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetSocialUserAuditLogSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetAuditLogs([FromServices] SessionSocialUserManagement    __SessionSocialUserManagement,
                                                      [FromServices] SocialUserAuditLogManagement   __SocialUserAuditLogManagement,
                                                      [FromServices] SocialUserManagement           __SocialUserManagement,
                                                      [FromHeader(Name = "session_token")] string   SessionToken,
                                                      [FromQuery(Name = "type")] string             Type,
                                                      [FromQuery(Name = "key")] string              Key         = default,  // post_id | comment_id
                                                      [FromQuery(Name = "start")] int               Start       = 0,
                                                      [FromQuery(Name = "size")] int                Size        = 20,
                                                      [FromQuery(Name = "search_term")] string      SearchTerm  = default)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SocialUserManagement,
                __SocialUserAuditLogManagement,
                __SessionSocialUserManagement
            );
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

                #region Validate params
                AddLogParam("start",       Start);
                AddLogParam("size",        Size);
                AddLogParam("type",        Type);
                AddLogParam("key",         Key);
                AddLogParam("search_term", SearchTerm);
                if (Start < 0 || Size < 1) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                #region Get all audit logs
                var (Logs, TotalSize)= await __SocialUserAuditLogManagement.GetAuditLogs(Session.UserId,
                                                                                         Type,
                                                                                         Key,
                                                                                         Start,
                                                                                         Size,
                                                                                         SearchTerm);

                List<JObject> RawRet = new();
                Logs.ForEach(e => RawRet.Add(e.GetJsonObject()));
                var Ret = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(RawRet));
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "logs",       Ret },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
