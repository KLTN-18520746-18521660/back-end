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

namespace CoreApi.Controllers.Social.Post
{
    [ApiController]
    [Route("/api/post")]
    public class ActionWithPostController : BaseController
    {
        protected readonly string[] ValidActions = new string[]{
            "like",
            "unlike",
            "dislike",
            "undislike",
            "save",
            "unsave",
            "follow",
            "unfollow",
        };

        public ActionWithPostController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpPost("{post_slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ActionPost([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                    [FromServices] SocialPostManagement         __SocialPostManagement,
                                                    [FromServices] NotificationsManagement      __NotificationsManagement,
                                                    [FromRoute(Name = "post_slug")] string      __PostSlug,
                                                    [FromQuery(Name = "action")] string         Action,
                                                    [FromHeader(Name = "session_token")] string SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SessionSocialUserManagement, __SocialPostManagement);
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
                AddLogParam("post_slug", __PostSlug);
                AddLogParam("action", Action);
                if (__PostSlug == default || __PostSlug.Trim() == string.Empty) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if (!ValidActions.Contains(Action)) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion


                #region Get post info
                var (Post, Error) = await __SocialPostManagement.FindPostBySlug(__PostSlug.Trim(), Session.UserId);

                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND || Error == ErrorCodes.USER_IS_NOT_OWNER) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "post" });
                    }

                    throw new Exception($"FindPostBySlug failed, ErrorCode: { Error }");
                }
                #endregion

                if (await __SocialPostManagement.IsContainsAction(Post.Id, Session.UserId, Action)) {
                    return Problem(400, RESPONSE_MESSAGES.ACTION_HAS_BEEN_TAKEN, new string[]{ Action });
                }
                NotificationSenderAction notificationAction = NotificationSenderAction.INVALID_ACTION;
                switch (Action) {
                    case "like":
                        Error = await __SocialPostManagement.Like(Post.Id, Session.UserId);
                        notificationAction = NotificationSenderAction.LIKE_POST;
                        break;
                    case "unlike":
                        Error = await __SocialPostManagement.UnLike(Post.Id, Session.UserId);
                        break;
                    case "dislike":
                        Error = await __SocialPostManagement.DisLike(Post.Id, Session.UserId);
                        break;
                    case "undislike":
                        Error = await __SocialPostManagement.UnDisLike(Post.Id, Session.UserId);
                        break;
                    case "save":
                        Error = await __SocialPostManagement.Save(Post.Id, Session.UserId);
                        break;
                    case "unsave":
                        Error = await __SocialPostManagement.UnSave(Post.Id, Session.UserId);
                        break;
                    case "follow":
                        Error = await __SocialPostManagement.Follow(Post.Id, Session.UserId);
                        break;
                    case "unfollow":
                        Error = await __SocialPostManagement.UnFollow(Post.Id, Session.UserId);
                        break;
                    default:
                        return Problem(400, RESPONSE_MESSAGES.INVALID_ACTION, new string[]{ Action });
                }

                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"{ Action } post failed, ErrorCode: { Error }");
                }

                if (notificationAction != NotificationSenderAction.INVALID_ACTION) {
                    await __NotificationsManagement.SendNotification(
                        NotificationType.ACTION_WITH_POST,
                        new PostNotificationModel(notificationAction,
                                                  Session.UserId,
                                                  default){
                            PostId = Post.Id,
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
