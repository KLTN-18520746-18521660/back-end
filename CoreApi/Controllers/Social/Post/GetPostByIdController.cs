using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Post
{
    [ApiController]
    [Route("/api/post")]
    public class GetPostByIdController : BaseController
    {
        public GetPostByIdController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpGet("id/{post_id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPostById([FromServices] SessionSocialUserManagement     __SessionSocialUserManagement,
                                                     [FromServices] SocialPostManagement            __SocialPostManagement,
                                                     [FromRoute(Name = "post_id")] long             __PostId,
                                                     [FromHeader(Name = "session_token")] string    SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionSocialUserManagement,
                __SocialPostManagement
            );
            #endregion
            try {
                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
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
                    throw new Exception($"FindPostById failed. Post_id: { __PostId }, ErrorCode: { Error} ");
                }

                if (Post.Owner != Session.UserId) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "post" });
                };
                #endregion

                var Ret = Post.GetJsonObject();
                Ret.Add("actions", Utils.ObjectToJsonToken(Post.GetActionByUser(Session.UserId)));
                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "post", Ret },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
