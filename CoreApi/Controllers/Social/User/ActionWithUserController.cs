using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace CoreApi.Controllers.Social.User
{
    [ApiController]
    [Route("/api/user")]
    public class ActionWithUserController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        protected readonly string[] ValidActions = new string[]{
            "follow",
            "unfollow",
        };

        public ActionWithUserController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "ActionWithUser";
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

        [HttpPost("{user_name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ActionWithUser([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                        [FromServices] SocialUserManagement __SocialUserManagement,
                                                        [FromServices] NotificationsManagement __NotificationsManagement,
                                                        [FromRoute] string user_name,
                                                        [FromQuery] string action,
                                                        [FromHeader] string session_token)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (user_name == default || user_name.Trim() == string.Empty) {
                    return Problem(400, "Invalid request.");
                }
                if (!ValidActions.Contains(action)) {
                    return Problem(400, "Invalid params.");
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

                #region Get user-des info
                SocialUser user_des     = default;
                var error               = ErrorCodes.NO_ERROR;
                (user_des, error) = await __SocialUserManagement.FindUser(user_name, false);

                if (error != ErrorCodes.NO_ERROR && error != ErrorCodes.USER_IS_NOT_OWNER) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found user destination.");
                    }

                    throw new Exception($"FindUser failed, ErrorCode: { error }, user_name: { user_name }");
                }
                if (user_des.Id == session.UserId) {
                    return Problem(400, "Not allow.");
                }
                #endregion

                if (await __SocialUserManagement.IsContainsAction(user_des.Id, session.UserId, action)) {
                    return Problem(400, $"User already { action } this user.");
                }
                NotificationSenderAction notificationAction = NotificationSenderAction.INVALID_ACTION;
                switch (action) {
                    case "follow":
                        error = await __SocialUserManagement.Follow(user_des.Id, session.UserId);
                        notificationAction = NotificationSenderAction.FOLLOW_USER;
                        break;
                    case "unfollow":
                        error = await __SocialUserManagement.UnFollow(user_des.Id, session.UserId);
                        break;
                    default:
                        return Problem(400, "Invalid action.");
                }

                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"{ action } post Failed, ErrorCode: { error }");
                }

                LogDebug($"Action with post ok, action: { action }, user_id: { session.UserId }");
                if (notificationAction != NotificationSenderAction.INVALID_ACTION) {
                    await __NotificationsManagement.SendNotification(
                        NotificationType.ACTION_WITH_USER,
                        new UserNotificationModel(notificationAction,
                                                  session.UserId,
                                                  default){
                            UserId = user_des.Id
                        }
                    );
                }

                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
