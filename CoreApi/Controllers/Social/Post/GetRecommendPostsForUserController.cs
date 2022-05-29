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
    public class GetRecommendPostsForUserController : BaseController
    {
        public GetRecommendPostsForUserController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpGet("recommend")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPostsByUserFollowing([FromServices] SessionSocialUserManagement     __SessionSocialUserManagement,
                                                                 [FromServices] SocialCategoryManagement        __SocialCategoryManagement,
                                                                 [FromServices] SocialUserManagement            __SocialUserManagement,
                                                                 [FromServices] SocialPostManagement            __SocialPostManagement,
                                                                 [FromServices] SocialTagManagement             __SocialTagManagement,
                                                                 [FromHeader(Name = "session_token")] string    SessionToken,
                                                                 [FromQuery(Name = "start")] int                Start       = 0,
                                                                 [FromQuery(Name = "size")] int                 Size        = 20)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionSocialUserManagement,
                __SocialUserManagement,
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
                AddLogParam("start", Start);
                AddLogParam("size", Size);
                if (Start < 0 || Size < 1) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                #region Get posts
                var (Posts, TotalSize, Error) = await __SocialPostManagement
                    .GetRecommendPostsForUser(
                        Session.UserId,
                        Start,
                        Size
                    );
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetRecommendPostsForUser failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                var Ret = new List<JObject>();
                Posts.ForEach(e => {
                    var Obj = e.GetPublicShortJsonObject();
                    Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    Ret.Add(Obj);
                });

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
