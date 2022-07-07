using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Post
{
    [ApiController]
    [Route("/api/post")]
    public class CreatePostController : BaseController
    {
        public CreatePostController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        /// <summary>
        /// Create new post
        /// </summary>
        /// <returns><b>Social user of session_token</b></returns>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="__SocialPostManagement"></param>
        /// <param name="__SocialUserManagement"></param>
        /// <param name="__SocialCategoryManagement"></param>
        /// <param name="__SocialTagManagement"></param>
        /// <param name="__NotificationsManagement"></param>
        /// <param name="__ParserModel"></param>
        /// <param name="SessionToken"></param>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header 'session_token'.
        /// - Body request have optional fields: 'thumbnail', 'short_content'
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> Social session of user.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Session not found.</li>
        /// <li>Mismatch user id.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="401">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="403">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Missing header session_token.</li>
        /// <li>Header session_token is invalid.</li>
        /// <li>User not have full permission on right 'post'.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> CreatePost([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                    [FromServices] SocialPostManagement         __SocialPostManagement,
                                                    [FromServices] SocialUserManagement         __SocialUserManagement,
                                                    [FromServices] SocialCategoryManagement     __SocialCategoryManagement,
                                                    [FromServices] SocialTagManagement          __SocialTagManagement,
                                                    [FromServices] NotificationsManagement      __NotificationsManagement,
                                                    [FromBody] ParserSocialPost                 __ParserModel,
                                                    [FromHeader(Name = "session_token")] string SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionSocialUserManagement,
                __SocialCategoryManagement,
                __SocialPostManagement,
                __SocialUserManagement,
                __SocialTagManagement
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

                #region Validate permission
                if (await __SocialUserManagement.HaveFullPermission(Session.UserId, SOCIAL_RIGHTS.POST) == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(403, RESPONSE_MESSAGES.USER_DOES_NOT_HAVE_PERMISSION, new string[]{ "create new post" });
                }
                #endregion

                var Post = new SocialPost();
                Post.Parse(__ParserModel, out var ErrParser);
                Post.Owner = Session.UserId;
                // post.OwnerNavigation = session.User;
                if (ErrParser != string.Empty) {
                    throw new Exception($"Parse social post model failed, error: { ErrParser }");
                }

                #region validate params
                if (!await __SocialCategoryManagement.IsExistingCategories(__ParserModel.categories)) {
                    return Problem(400, RESPONSE_MESSAGES.ALREADY_EXIST, new string[]{ "Category" });
                }
                var (IsValidTags, Error) = await __SocialTagManagement.IsValidTags(__ParserModel.tags);
                if (!IsValidTags) {
                    AddLogParam("tags", __ParserModel.tags);
                    if (Error == ErrorCodes.INVALID_PARAMS) {
                        return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                    }
                    throw new Exception($"IsValidTags failed, ErrorCode: { Error }");
                }
                #endregion

                Error = await __SocialPostManagement.AddNewPost(__ParserModel, Post, Session.UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"AddNewPost failed, ErrorCode: { Error }");
                }

                // await __NotificationsManagement.SendNotification(
                //     NotificationType.ACTION_WITH_POST,
                //     new PostNotificationModel(NotificationSenderAction.NEW_POST,
                //                               Session.UserId,
                //                               default){
                //         PostId = Post.Id,
                //     }
                // );

                return Ok(201, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "post_id", Post.Id },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
