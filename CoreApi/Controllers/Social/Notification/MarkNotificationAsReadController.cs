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
    [Route("/api/notification/read")]
    public class MarkNotificationAsReadController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public MarkNotificationAsReadController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "MarkNotificationAsRead";
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
        /// <param name="notification_id"></param>
        /// <param name="session_token"></param>
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
        public async Task<IActionResult> MarkNotificationAsRead([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                                [FromServices] SocialUserManagement __SocialUserManagement,
                                                                [FromServices] NotificationsManagement __NotificationsManagement,
                                                                [FromRoute] long notification_id,
                                                                [FromHeader] string session_token)
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
                if (notification_id == default || notification_id <= 0) {
                    return Problem(400, $"Invalid params.");
                }
                #endregion

                #region Get session
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                var (__session, errRet) = await GetSessionToken(__SessionSocialUserManagement, EXPIRY_TIME, EXTENSION_TIME, session_token);
                if (errRet != default) {
                    return errRet;
                }
                if (__session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var session = __session as SessionSocialUser;
                #endregion

                var error = await __NotificationsManagement.MarkNotificationAsRead(session.UserId, notification_id);
                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Notification not found.");
                    }
                    if (error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, "Notification is read by user.");
                    }
                    if (error == ErrorCodes.INVALID_ACTION) {
                        return Problem(400, "Invalid action");
                    }
                    throw new Exception($"MarkNotificationAsRead Failed. ErrorCode: { error }");
                }

                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }

        [HttpPost("all")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> MarkNotificationsAsRead([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                                 [FromServices] SocialUserManagement __SocialUserManagement,
                                                                 [FromServices] NotificationsManagement __NotificationsManagement,
                                                                 [FromHeader] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                var (__session, errRet) = await GetSessionToken(__SessionSocialUserManagement, EXPIRY_TIME, EXTENSION_TIME, session_token);
                if (errRet != default) {
                    return errRet;
                }
                if (__session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var session = __session as SessionSocialUser;
                #endregion

                var error = await __NotificationsManagement.MarkNotificationsAsRead(session.UserId);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"MarkNotificationsAsRead Failed. ErrorCode: { error }");
                }

                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
