using Common;
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

namespace CoreApi.Controllers.Admin.AuditLog
{
    [ApiController]
    [Route("/api/admin/sociallog")]
    public class GetSocialAuditLogController : BaseController
    {
        public GetSocialAuditLogController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
            // ControllerName = "GetSocialAuditLog";
        }

        /// <summary>
        /// Get social auditlog
        /// </summary>
        /// <param name="__AdminUserManagement"></param>
        /// <param name="__SocialAuditLogManagement"></param>
        /// <param name="__SessionAdminUserManagement"></param>
        /// <param name="SessionToken"></param>
        /// <param name="Start"></param>
        /// <param name="Size"></param>
        /// <param name="SearchTerm"></param>
        /// <returns><b>List social auditlog</b></returns>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header or cookie 'session_token_admin'.
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
        /// <b>Error case <i>(Server auto send response with will clear cookie 'session_token_admin')</i>, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
        /// <li>Session not found.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="403">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>User doesn't have permission to read auditlog.</li>
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAdminAuditLogSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetAuditLogs([FromServices] AdminUserManagement                __AdminUserManagement,
                                                      [FromServices] SocialAuditLogManagement           __SocialAuditLogManagement,
                                                      [FromServices] SessionAdminUserManagement         __SessionAdminUserManagement,
                                                      [FromHeader(Name = "session_token_admin")] string SessionToken,
                                                      [FromQuery(Name = "start")] int                   Start = 0,
                                                      [FromQuery(Name = "size")] int                    Size = 20,
                                                      [FromQuery(Name = "search_term")] string          SearchTerm = default)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __AdminUserManagement,
                __SocialAuditLogManagement,
                __SessionAdminUserManagement
            );
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

                #region Check Permission
                var Error = __AdminUserManagement.HaveReadPermission(Session.User.Rights, ADMIN_RIGHTS.LOG);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(403, "User doesn't have permission to see admin audit log.");
                }
                #endregion

                #region Get all audit logs
                var (Logs, TotalSize) = await __SocialAuditLogManagement.GetAuditLogs(Start, Size, SearchTerm);
                var RawRet              = new List<JObject>();
                Logs.ForEach(e => RawRet.Add(e.GetJsonObject()));
                AddLogParam("total_size", TotalSize);
                var Ret = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(RawRet));
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    return Problem(400, $"Invalid request params start: { Start }. Total size is { TotalSize }");
                }
                #endregion

                return Ok(200, "OK", new JObject(){
                    { "logs",       Ret },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
