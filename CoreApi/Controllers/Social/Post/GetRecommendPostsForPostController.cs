using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using DatabaseAccess.Common.Status;
using Newtonsoft.Json;

namespace CoreApi.Controllers.Social.Post
{
    [ApiController]
    [Route("/api/post")]
    public class GetRecommendPostsForPostController : BaseController
    {
        public GetRecommendPostsForPostController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpGet("recommend/{post_slug}")]
        public async Task<IActionResult> GetRecommendPostsForPost([FromServices] SessionSocialUserManagement    __SessionSocialUserManagement,
                                                                  [FromServices] SocialPostManagement           __SocialPostManagement,
                                                                  [FromRoute(Name = "post_slug")] string        __PostSlug,
                                                                  [FromHeader(Name = "session_token")] string   SessionToken,
                                                                  [FromQuery(Name = "start")] int               Start   = 0,
                                                                  [FromQuery(Name = "size")] int                Size    = 5)
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
                AddLogParam("start",        Start);
                AddLogParam("size",         Size);
                AddLogParam("post_slug",    __PostSlug);
                if (__PostSlug == default || __PostSlug.Trim() == string.Empty) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if (Start < 0 || Size < 1) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                #region Get post info
                var (Post, Error) = await __SocialPostManagement.FindPostBySlug(__PostSlug.Trim(), IsValidSession ? Session.UserId : default);

                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND || Error == ErrorCodes.USER_IS_NOT_OWNER) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "post" });
                    }

                    throw new Exception($"FindPostBySlug failed, ErrorCode: { Error }");
                }
                #endregion

                #region Get posts
                var Posts       = new List<SocialPost>();
                var TotalSize   = 0;
                (Posts, TotalSize, Error) = await __SocialPostManagement
                    .GetRecommendPostsForPost(
                        Post.Id,
                        Start,
                        Size,
                        IsValidSession ? Session.UserId : default
                    );
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetRecommendPostsForPost failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                var Ret = new List<JObject>();
                if (IsValidSession) {
                    Posts.ForEach(e => {
                        var Obj = e.GetPublicShortJsonObject(IsValidSession ? Session.UserId : default);
                        Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                        Ret.Add(Obj);
                    });
                }

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "posts",      Utils.ObjectToJsonToken(Ret) },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
