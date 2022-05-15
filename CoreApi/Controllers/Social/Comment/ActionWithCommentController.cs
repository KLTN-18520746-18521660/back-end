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
        protected readonly string[] ValidActions = new string[]{
            "like",
            "unlike",
            "dislike",
            "undislike",
        };

        public ActionWithCommentController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "ActionWithComment";
        }

        [HttpPost("{comment_id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ActionComment([FromServices] SessionSocialUserManagement   __SessionSocialUserManagement,
                                                       [FromServices] SocialCommentManagement       __SocialCommentManagement,
                                                       [FromServices] SocialPostManagement          __SocialPostManagement,
                                                       [FromServices] NotificationsManagement       __NotificationsManagement,
                                                       [FromRoute(Name = "commment_id")] long       __CommentId,
                                                       [FromQuery(Name = "action")] string          Action,
                                                       [FromHeader(Name = "session_token")] string  SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialCommentManagement.SetTraceId(TraceId);
            __SocialPostManagement.SetTraceId(TraceId);
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
                if (__CommentId == default || __CommentId <= 0) {
                    return Problem(400, "Invalid request.");
                }
                if (!ValidActions.Contains(Action)) {
                    return Problem(400, "Invalid params.");
                }
                #endregion

                #region Get comment info
                var (Comment, Error) = await __SocialCommentManagement.FindCommentById(__CommentId);

                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        LogWarning($"Not found comment, comment_id: { __CommentId }");
                        return Problem(404, "Not found comment.");
                    }

                    throw new Exception($"FindCommentById failed, ErrorCode: { Error }");
                }
                #endregion

                if (await __SocialCommentManagement.IsContainsAction(__CommentId, Session.UserId, Action)) {
                    LogWarning($"Action is already exists, action: { Action }, comment_id: { __CommentId }, user_id: { Session.UserId }");
                    return Problem(400, $"User already { Action } this comment.");
                }

                NotificationSenderAction NotificationAction = NotificationSenderAction.INVALID_ACTION;
                switch (Action) {
                    case "like":
                        Error = await __SocialCommentManagement.Like(__CommentId, Session.UserId);
                        NotificationAction = NotificationSenderAction.LIKE_COMMENT;
                        break;
                    case "unlike":
                        Error = await __SocialCommentManagement.UnLike(__CommentId, Session.UserId);
                        break;
                    case "dislike":
                        Error = await __SocialCommentManagement.DisLike(__CommentId, Session.UserId);
                        break;
                    case "undislike":
                        Error = await __SocialCommentManagement.UnDisLike(__CommentId, Session.UserId);
                        break;
                    default:
                        return Problem(400, "Invalid action.");
                }

                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"{ Action } comment Failed, ErrorCode: { Error }");
                }

                if (NotificationAction != NotificationSenderAction.INVALID_ACTION) {
                    await __NotificationsManagement.SendNotification(
                        NotificationType.ACTION_WITH_COMMENT,
                        new CommentNotificationModel(NotificationAction,
                                                     Session.UserId,
                                                     default){
                            CommentId = Comment.Id
                        }
                    );
                }

                LogDebug($"Action with comment success, action: { Action }, comment_id: { __CommentId }");
                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }
    }
}
