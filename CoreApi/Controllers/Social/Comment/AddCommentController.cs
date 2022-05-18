using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Comment
{
    [ApiController]
    [Route("/api/comment")]
    public class AddCommentController : BaseController
    {
        public AddCommentController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpPost("post/{post_slug}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPostBySlug([FromServices] SessionSocialUserManagement   __SessionSocialUserManagement,
                                                       [FromServices] SocialCommentManagement       __SocialCommentManagement,
                                                       [FromServices] SocialPostManagement          __SocialPostManagement,
                                                       [FromServices] NotificationsManagement       __NotificationsManagement,
                                                       [FromBody] ParserSocialComment               __ParserModel,
                                                       [FromRoute(Name = "post_slug")] string       __PostSlug,
                                                       [FromHeader(Name = "session_token")] string  SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SessionSocialUserManagement);
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

                #region Validate slug
                AddLogParam("post_slug", __PostSlug);
                if (__PostSlug == default || __PostSlug.Trim() == string.Empty) {
                    return Problem(400, "Invalid request.");
                }
                #endregion

                #region Get post info
                var (Post, Error) = await __SocialPostManagement.FindPostBySlug(__PostSlug.Trim());

                if (Error != ErrorCodes.NO_ERROR && Error != ErrorCodes.USER_IS_NOT_OWNER) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found post.");
                    }

                    throw new Exception($"FindPostBySlug failed, ErrorCode: { Error }");
                }
                #endregion

                var Comment = new SocialComment();
                Comment.Parse(__ParserModel, out var ErrParser);
                Comment.Owner = Session.UserId;
                Comment.PostId = Post.Id;

                if (ErrParser != string.Empty) {
                    throw new Exception($"Parse social comment model failed, error: { ErrParser }");
                }

                if (Comment.ParentId != default) {
                    AddLogParam("parent_comment_id", Comment.ParentId);
                    SocialComment ParentComment = default;
                    (ParentComment, Error) = await __SocialCommentManagement.FindCommentById((long) Comment.ParentId);
                    if (Error != ErrorCodes.NO_ERROR) {
                        return Problem(404, "Not found parent comment.");
                    }
                    if (ParentComment.PostId != Post.Id) {
                        return Problem(400, "Invalid parent comment.");
                    }
                }

                Error = await __SocialCommentManagement.AddComment(Comment);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"AddComment failed, ErrorCode: { Error }");
                }

                #region Handle notification
                var notifications = new List<(NotificationType, BaseNotificationSenderModel)>{
                    (
                        NotificationType.ACTION_WITH_COMMENT,
                        new CommentNotificationModel(NotificationSenderAction.NEW_COMMENT,
                                                     Session.UserId,
                                                     default){
                            CommentId = Comment.Id
                        }
                    )
                };
                if (Comment.ParentId != default) {
                    notifications.Add(
                        (
                            NotificationType.ACTION_WITH_COMMENT,
                            new CommentNotificationModel(NotificationSenderAction.REPLY_COMMENT,
                                                         Session.UserId,
                                                         default){
                                CommentId = Comment.Id
                            }
                        )
                    );
                }
                await __NotificationsManagement.SendNotifications(notifications.ToArray());
                #endregion

                Comment.OwnerNavigation = Session.User;
                Comment.Post = Post;

                var Ret = Comment.GetPublicJsonObject();
                Ret.Add("actions", Utils.ObjectToJsonToken(Comment.GetActionByUser(Session.UserId)));

                return Ok(201, "OK", new JObject(){
                    { "comment", Ret },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
