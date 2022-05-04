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

namespace CoreApi.Controllers.Social.AuditLog
{
    [ApiController]
    [Route("/api/auditlog")]
    public class GetSocialAuditLogController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minutes
        #endregion
        protected string[] AllowActions = new string[]{
            "comment",
            "post",
            "user",
        };

        public GetSocialAuditLogController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "GetSocialAuditLog";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = string.Empty;
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
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
        /// Get social user auditlog
        /// </summary>
        /// <param name="__SocialUserManagement"></param>
        /// <param name="__SocialUserAuditLogManagement"></param>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="session_token"></param>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="search_term"></param>
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
        public async Task<IActionResult> GetAuditLogs([FromServices] SocialUserManagement __SocialUserManagement,
                                                      [FromServices] SocialUserAuditLogManagement __SocialUserAuditLogManagement,
                                                      [FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                      [FromHeader] string session_token,
                                                      [FromQuery] string type,
                                                      [FromQuery] string key = default,       // post_id | comment_id
                                                      [FromQuery] int start = 0,
                                                      [FromQuery] int size = 20,
                                                      [FromQuery] string search_term = default)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SocialUserManagement.SetTraceId(TraceId);
            __SocialUserAuditLogManagement.SetTraceId(TraceId);
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (start < 0 || size < 1) {
                    LogDebug($"Bad request params.");
                    return Problem(400, "Bad request params.");
                }
                #endregion
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

                #region Find session for use
                SessionSocialUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);
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
                    throw new Exception($"FindSessionForUse Failed. ErrorCode: { error }");
                }
                #endregion

                #region Get all audit logs
                var user = session.User;
                var (logs, totalSize)= await __SocialUserAuditLogManagement.GetAuditLogs(user.Id, type, start, size, search_term);

                List<JObject> rawReturn = new();
                logs.ForEach(e => rawReturn.Add(e.GetJsonObject()));
                var ret = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(rawReturn));
                #endregion

                #region Validate params: start, size, total_size
                if (totalSize != 0 && start >= totalSize) {
                    LogWarning($"Invalid request params for get audit log, start: { start }, size: { size }, search_term: { search_term }, total_size: { totalSize }");
                    return Problem(400, $"Invalid request params start: { start }. Total size is { totalSize }");
                }
                #endregion

                LogInformation($"Get social user auditlogs success, user_name: { user.UserName }, start: { start }, size: { size }, search_term: { search_term }");
                return Ok(200, "OK", new JObject(){
                    { "logs", ret },
                    { "total_size", totalSize },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
