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

namespace CoreApi.Controllers.Social.Comment
{
    [ApiController]
    [Route("/api/comment")]
    public class ActionWithCommentController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        protected readonly string[] ValidActions = new string[]{
            "like",
            "unlike",
            "dislike",
            "undislike",
        };

        public ActionWithCommentController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "ActionWithComment";
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

        [HttpPost("{comment_id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ActionComment([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                       [FromServices] SocialCommentManagement __SocialCommentManagement,
                                                       [FromServices] SocialPostManagement __SocialPostManagement,
                                                       [FromServices] NotificationsManagement __NotificationsManagement,
                                                       [FromRoute] long comment_id,
                                                       [FromQuery] string action,
                                                       [FromHeader] string session_token)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialCommentManagement.SetTraceId(TraceId);
            __SocialPostManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (comment_id == default || comment_id <= 0) {
                    return Problem(400, "Invalid request.");
                }
                if (!ValidActions.Contains(action)) {
                    return Problem(400, "Invalid params.");
                }
                #endregion

                #region Get session token
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
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

                #region Get comment info
                SocialComment comment = default;
                (comment, error) = await __SocialCommentManagement.FindCommentById(comment_id);

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found comment.");
                    }

                    throw new Exception($"FindCommentById failed, ErrorCode: { error }");
                }
                #endregion

                if (await __SocialCommentManagement.IsContainsAction(comment_id, session.UserId, action)) {
                    return Problem(400, $"User already { action } this comment.");
                }
                NotificationSenderAction notificationAction = NotificationSenderAction.INVALID_ACTION;
                switch (action) {
                    case "like":
                        error = await __SocialCommentManagement.Like(comment_id, session.UserId);
                        notificationAction = NotificationSenderAction.LIKE_COMMENT;
                        break;
                    case "unlike":
                        error = await __SocialCommentManagement.UnLike(comment_id, session.UserId);
                        break;
                    case "dislike":
                        error = await __SocialCommentManagement.DisLike(comment_id, session.UserId);
                        break;
                    case "undislike":
                        error = await __SocialCommentManagement.UnDisLike(comment_id, session.UserId);
                        break;
                    default:
                        return Problem(400, "Invalid action.");
                }

                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"{ action } comment Failed, ErrorCode: { error }");
                }

                if (notificationAction != NotificationSenderAction.INVALID_ACTION) {
                    await __NotificationsManagement.SendNotification(
                        NotificationType.ACTION_WITH_COMMENT,
                        new CommentNotificationModel(notificationAction,
                                                     session.UserId,
                                                     default){
                            CommentId = comment.Id
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
