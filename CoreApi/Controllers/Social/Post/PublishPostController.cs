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

namespace CoreApi.Controllers.Social.Post
{
    [ApiController]
    [Route("/api/post")]
    public class PublishPostController : BaseController
    {
        public PublishPostController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpPost("publish/id/{post_id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> PublishPost([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                     [FromServices] SocialPostManagement __SocialPostManagement,
                                                     [FromServices] NotificationsManagement __NotificationsManagement,
                                                     [FromRoute(Name = "post_id")] long __PostId,
                                                     [FromHeader(Name = "session_token")] string SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionSocialUserManagement,
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
                AddLogParam("post_id", __PostId);
                if (__PostId <= 0) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                #region Get post info
                var (Post, Error) = await __SocialPostManagement.FindPostById(__PostId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "post" });
                    }
                    throw new Exception($"FindPostById failed. Post_id: { __PostId }, ErrorCode: { Error } ");
                }

                if (Post.Owner != Session.UserId) {
                    AddLogParam("post_owner", Post.Owner);
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "post" });
                }
                if (Post.Status.Type == StatusType.Deleted) {
                    AddLogParam("post_status", Post.StatusStr);
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "post" });
                }
                if (__SocialPostManagement.ValidateChangeStatusAction(Post.Status.Type, StatusType.Pending) == ErrorCodes.INVALID_ACTION) {
                    return Problem(400, RESPONSE_MESSAGES.NOT_ALLOW_TO_DO, new string[]{ "publish post" });
                }
                #endregion

                Error = await __SocialPostManagement.PublishPostPrivate(Post.Id, Post.Owner);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"PublishPostPrivate failed, ErrorCode: { Error }");
                }

                return Ok(200, RESPONSE_MESSAGES.OK);
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
