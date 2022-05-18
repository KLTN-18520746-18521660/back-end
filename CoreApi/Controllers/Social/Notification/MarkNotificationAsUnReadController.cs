using Common;
using CoreApi.Common.Base;
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
    [Route("/api/notification/unread")]
    public class MarkNotificationAsUnReadController : BaseController
    {
        public MarkNotificationAsUnReadController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        /// <summary>
        /// Get all post attach to user
        /// </summary>
        /// <returns><b>Social user of session_token</b></returns>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="__SocialUserManagement"></param>
        /// <param name="__NotificationsManagement"></param>
        /// <param name="__NotificationId"></param>
        /// <param name="SessionToken"></param>
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
        [HttpPost("id/{notification_id}")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> MarkNotificationAsUnRead([FromServices] SessionSocialUserManagement    __SessionSocialUserManagement,
                                                                  [FromServices] SocialUserManagement           __SocialUserManagement,
                                                                  [FromServices] NotificationsManagement        __NotificationsManagement,
                                                                  [FromRoute(Name = "notification_id")] long    __NotificationId,
                                                                  [FromHeader(Name = "session_token")] string   SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SessionSocialUserManagement, __SocialUserManagement);
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
                AddLogParam("notification_id", __NotificationId);
                if (__NotificationId == default || __NotificationId <= 0) {
                    return Problem(400, $"Invalid params.");
                }
                #endregion

                var Error = await __NotificationsManagement.MarkNotificationAsUnRead(Session.UserId, __NotificationId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Notification not found.");
                    }
                    if (Error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, "Notification isn't read by user.");
                    }
                    if (Error == ErrorCodes.INVALID_ACTION) {
                        return Problem(400, "Invalid action");
                    }
                    throw new Exception($"MarkNotificationAsUnRead Failed. ErrorCode: { Error }");
                }

                return Ok(200, "OK");
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }

        [HttpPost("all")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> MarkNotificationsAsUnRead([FromServices] SessionSocialUserManagement   __SessionSocialUserManagement,
                                                                   [FromServices] SocialUserManagement          __SocialUserManagement,
                                                                   [FromServices] NotificationsManagement       __NotificationsManagement,
                                                                   [FromHeader(Name = "session_token")] string  SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SessionSocialUserManagement, __SocialUserManagement);
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

                var Error = await __NotificationsManagement.MarkNotificationsAsUnRead(Session.UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"MarkNotificationsAsUnRead Failed. ErrorCode: { Error }");
                }

                return Ok(200, "OK");
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
