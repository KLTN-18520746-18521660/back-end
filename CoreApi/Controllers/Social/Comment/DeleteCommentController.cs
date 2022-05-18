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

namespace CoreApi.Controllers.Social.Comment
{
    [ApiController]
    [Route("/api/comment")]
    public class DeleteCommentController : BaseController
    {
        public DeleteCommentController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpDelete("{comment_id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> DeleteComment([FromServices] SessionSocialUserManagement   __SessionSocialUserManagement,
                                                       [FromServices] SocialCommentManagement       __SocialCommentManagement,
                                                       [FromServices] SocialPostManagement          __SocialPostManagement,
                                                       [FromRoute(Name = "comment_id")] long        __CommentId,
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
                AddLogParam("comment_id", __CommentId);
                if (__CommentId == default || __CommentId <= 0) {
                    return Problem(400, "Invalid request.");
                }
                #endregion

                #region Get comment info
                var (Comment, Error) = await __SocialCommentManagement.FindCommentById(__CommentId);

                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found comment.");
                    }

                    throw new Exception($"FindCommentById failed, ErrorCode: { Error }");
                }
                #endregion

                if (Comment.Owner != Session.UserId) {
                    AddLogParam("comment_owner", Comment.Owner);
                    return Problem(403, "Not allow.");
                }

                Error = await __SocialCommentManagement.DeleteComment(Comment);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"DeleteComment failed, ErrorCode: { Error }");
                }

                return Ok(200, "OK", new JObject(){
                    { "comment_id", Comment.Id },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
