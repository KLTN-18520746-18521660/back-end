using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public AddCommentController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "AddComment";
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

        [HttpPost("post/{post_slug}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPostBySlug([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                       [FromServices] SocialCommentManagement __SocialCommentManagement,
                                                       [FromServices] SocialPostManagement __SocialPostManagement,
                                                       [FromServices] NotificationsManagement __NotificationsManagement,
                                                       [FromRoute] string post_slug,
                                                       [FromHeader] string session_token,
                                                       [FromBody] ParserSocialComment Parser)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate slug
                if (post_slug == default || post_slug.Trim() == string.Empty) {
                    return Problem(400, "Invalid request.");
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

                #region Get post info
                var (post, error) = await __SocialPostManagement.FindPostBySlug(post_slug.Trim());

                if (error != ErrorCodes.NO_ERROR && error != ErrorCodes.USER_IS_NOT_OWNER) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found post.");
                    }

                    throw new Exception($"FindPostBySlug failed, ErrorCode: { error }");
                }
                #endregion

                var comment = new SocialComment();
                comment.Parse(Parser, out var errMsg);
                comment.Owner = session.UserId;
                comment.PostId = post.Id;

                if (errMsg != string.Empty) {
                    throw new Exception($"Parse social comment model failed, error: { errMsg }");
                }

                if (comment.ParentId != default) {
                    SocialComment parentComment = default;
                    (parentComment, error) = await __SocialCommentManagement.FindCommentById((long) comment.ParentId);
                    if (error != ErrorCodes.NO_ERROR || parentComment.PostId != post.Id) {
                        return Problem(400, "Invalid parent_id.");
                    }
                }

                error = await __SocialCommentManagement.AddComment(comment);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"AddComment failed, ErrorCode: { error }");
                }

                #region Handle notification
                var notifications = new List<(NotificationType, BaseNotificationSenderModel)>{
                    (
                        NotificationType.ACTION_WITH_COMMENT,
                        new CommentNotificationModel(NotificationSenderAction.NEW_COMMENT,
                                                     session.UserId,
                                                     default){
                            CommentId = comment.Id
                        }
                    )
                };
                if (comment.ParentId != default) {
                    notifications.Add(
                        (
                            NotificationType.ACTION_WITH_COMMENT,
                            new CommentNotificationModel(NotificationSenderAction.REPLY_COMMENT,
                                                         session.UserId,
                                                         default){
                                CommentId = comment.Id
                            }
                        )
                    );
                }
                await __NotificationsManagement.SendNotifications(notifications.ToArray());
                #endregion

                // SocialComment r_comment = default;
                // (r_comment, error) =  await (__SocialCommentManagement.FindCommentById(comment.Id));
                // if (error != ErrorCodes.NO_ERROR) {
                //     throw new Exception($"FindCommentById failed, ErrorCode: { error }");
                // }
                comment.OwnerNavigation = session.User;
                comment.Post = post;

                var ret = comment.GetPublicJsonObject();
                ret.Add("actions", Utils.ObjectToJsonToken(comment.GetActionByUser(session.UserId)));

                return Ok(201, "OK", new JObject(){
                    { "comment", ret },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
