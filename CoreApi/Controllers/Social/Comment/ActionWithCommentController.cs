using Common;
using CoreApi.Common.Base;
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
                                                       [FromRoute(Name = "comment_id")] long       __CommentId,
                                                       [FromQuery(Name = "action")] string          Action,
                                                       [FromHeader(Name = "session_token")] string  SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionSocialUserManagement,
                __SocialCommentManagement,
                __SocialPostManagement
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
                AddLogParam("comment_id",  __CommentId);
                AddLogParam("action",       Action);
                if (__CommentId == default || __CommentId <= 0) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if (!ValidActions.Contains(Action)) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                #region Get comment info
                var (Comment, Error) = await __SocialCommentManagement.FindCommentById(__CommentId);

                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "comment" });
                    }

                    throw new Exception($"FindCommentById failed, ErrorCode: { Error }");
                }
                #endregion

                if (await __SocialCommentManagement.IsContainsAction(__CommentId, Session.UserId, Action)) {
                    return Problem(400, RESPONSE_MESSAGES.ACTION_HAS_BEEN_TAKEN, new string[]{ Action });
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
                        return Problem(400, RESPONSE_MESSAGES.INVALID_ACTION, new string[]{ Action });
                }

                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"{ Action } comment failed, ErrorCode: { Error }");
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

                return Ok(200, RESPONSE_MESSAGES.OK);
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
