using Common;
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
    public class GetPostBySlugController : BaseController
    {
        public GetPostBySlugController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "GetPostBySlug";
        }

        /// <summary>
        /// Get post info by slug
        /// </summary>
        /// <returns><b>Social user of session_token</b></returns>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="__SocialPostManagement"></param>
        /// <param name="__PostSlug"></param>
        /// <param name="SessionToken"></param>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Header 'session_token' is optional.
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
        /// <li>Invalid slug.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpGet("{post_slug}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPostBySlug([FromServices] SessionSocialUserManagement   __SessionSocialUserManagement,
                                                       [FromServices] SocialPostManagement          __SocialPostManagement,
                                                       [FromRoute(Name = "post_slug")] string       __PostSlug,
                                                       [FromHeader(Name = "session_token")] string  SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialPostManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Validate params
                AddLogParam("post_slug", __PostSlug);
                if (__PostSlug == default || __PostSlug.Trim() == string.Empty) {
                    return Problem(400, "Invalid request.");
                }
                #endregion

                var (Post, Error) = await __SocialPostManagement.FindPostBySlug(__PostSlug.Trim(), IsValidSession ? Session.UserId : default);

                if (Error != ErrorCodes.NO_ERROR && Error != ErrorCodes.USER_IS_NOT_OWNER) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found post.");
                    }

                    throw new Exception($"FindPostBySlug failed, ErrorCode: { Error }");
                }
                var Ret = (Error == ErrorCodes.USER_IS_NOT_OWNER) ? Post.GetPublicJsonObject() : Post.GetJsonObject();

                // Add action if user is valid
                if (IsValidSession) {
                    Ret.Add("actions", Utils.ObjectToJsonToken(Post.GetActionByUser(Session.UserId)));
                }
                return Ok(200, "OK", new JObject(){
                    { "post",   Ret },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }

        [HttpGet("{post_slug}/statistic")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetStatisticPostBySlug([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                                [FromServices] SocialPostManagement         __SocialPostManagement,
                                                                [FromRoute(Name = "post_slug")] string      __PostSlug,
                                                                [FromHeader(Name = "session_token")] string SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Validate params
                AddLogParam("post_slug", __PostSlug);
                if (__PostSlug == default || __PostSlug.Trim() == string.Empty) {
                    return Problem(400, "Invalid request.");
                }
                #endregion

                var (Post, Error) = await __SocialPostManagement.FindPostBySlug(__PostSlug.Trim());

                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found post.");
                    }

                    throw new Exception($"FindPostBySlug failed, ErrorCode: { Error }");
                }
                var Ret = Post.GetPublicStatisticJsonObject();
                if (IsValidSession) {
                    Ret.Add("actions", Utils.ObjectToJsonToken(Post.GetActionByUser(Session.UserId)));
                }

                return Ok(200, "OK", new JObject(){
                    { "post", Ret },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
