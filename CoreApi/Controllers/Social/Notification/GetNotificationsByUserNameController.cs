using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using DatabaseAccess.Common.Status;

namespace CoreApi.Controllers.Social.Notification
{
    [ApiController]
    [Route("/api/notification")]
    public class GetNotificationsByUserNameController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetNotificationsByUserNameController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "GetNotificationsByUserName";
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
        /// Get all post attach to user
        /// </summary>
        /// <returns><b>Social user of session_token</b></returns>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="__SocialUserManagement"></param>
        /// <param name="__NotificationsManagement"></param>
        /// <param name="session_token"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="status"></param>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Header 'session_token' is optional.
        /// - If have session_token --> compare user is owner ?
        ///     - Is owner --> return full info of post (include post is pendding, reject, private)
        ///     - Else --> return info of public post (just post approve)
        /// - <i>Not allow search post of user have delete status.</i>
        /// - Must have query params for paging 'first', 'size'
        /// - Support query params 'status' (approve | pendding | reject | private) for filter
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> Social session of user.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Invalid slug.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpGet("")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetNotificationsByUserName([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                                    [FromServices] SocialUserManagement __SocialUserManagement,
                                                                    [FromServices] NotificationsManagement __NotificationsManagement,
                                                                    [FromHeader] string session_token,
                                                                    [FromQuery] int start = 0,
                                                                    [FromQuery] int size = 20,
                                                                    [FromQuery] string status = default)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                string[] statusArr = status == default ? default : status.Split(',');
                if (status != default) {
                    foreach (var statusStr in statusArr) {
                        var statusType = EntityStatus.StatusStringToType(statusStr);
                        if (statusType == default || statusType == StatusType.Deleted) {
                            return Problem(400, $"Invalid status: { statusStr }.");
                        }
                    }
                }
                #endregion

                #region Get session token
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                    return Problem(401, "Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    return Problem(401, "Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { session_token.Substring(0, 15) }");
                        return Problem(401, "Session not found.");
                    }
                    if (error == ErrorCodes.SESSION_HAS_EXPIRED) {
                        LogInformation($"Session has expired, session_token: { session_token.Substring(0, 15) }");
                        return Problem(401, "Session has expired.");
                    }
                    if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogWarning($"User has been locked, session_token: { session_token.Substring(0, 15) }");
                        return Problem(423, "You have been locked.");
                    }
                    throw new Exception($"FindSessionForUse Failed. ErrorCode: { error }");
                }
                #endregion

                #region Get notifications
                List<SocialNotification> notifications = default;
                int totalSize = default;
                (notifications, totalSize) = await __NotificationsManagement
                    .GetNotifications(
                        session.UserId,
                        start,
                        size,
                        statusArr
                    );
                #endregion

                #region Validate params: start, size, total_size
                if (totalSize != 0 && start >= totalSize) {
                    LogWarning($"Invalid request params for get posts, start: { start }, size: { size }, total_size: { totalSize }");
                    return Problem(400, $"Invalid request params start: { start }. Total size is { totalSize }");
                }
                #endregion

                var ret = new List<JObject>();
                notifications.ForEach(e => {
                    var obj = e.GetPublicJsonObject();
                    ret.Add(obj);
                });

                return Ok(200, "OK", new JObject(){
                    { "notifications", Utils.ObjectToJsonToken(ret) },
                    { "total_size", totalSize },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
