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
    public class GetTrendingPostsController : BaseController
    {
        private int[] AllowTimeTrending = new int[]{
            -1,     // all time
            7,      // week
            30,     // month
            180,    // 6 month
        };

        public GetTrendingPostsController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpGet("trending")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetTrendingPosts([FromServices] SessionSocialUserManagement    __SessionSocialUserManagement,
                                                          [FromServices] SocialCategoryManagement       __SocialCategoryManagement,
                                                          [FromServices] SocialUserManagement           __SocialUserManagement,
                                                          [FromServices] SocialPostManagement           __SocialPostManagement,
                                                          [FromServices] SocialTagManagement            __SocialTagManagement,
                                                          [FromHeader(Name = "session_token")] string   SessionToken,
                                                          [FromQuery(Name = "timem")] int               Time        = 7,
                                                          [FromQuery(Name = "start")] int               Start       = 0,
                                                          [FromQuery(Name = "size")] int                Size        = 20,
                                                          [FromQuery(Name = "size")] string             SearchTerm  = default,
                                                          [FromQuery(Name = "tags")] string             Tags        = default,
                                                          [FromQuery(Name = "categories")] string       Categories  = default)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionSocialUserManagement,
                __SocialCategoryManagement,
                __SocialUserManagement,
                __SocialPostManagement,
                __SocialTagManagement
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
                AddLogParam("start", Start);
                AddLogParam("size", Size);
                AddLogParam("search_term", SearchTerm);
                AddLogParam("categories", Categories);
                AddLogParam("tags", Tags);
                var CategoriesArr   = Categories == default ? default : Categories.Split(',');
                var TagsArr         = Tags == default ? default : Tags.Split(',');
                if (!AllowTimeTrending.Contains(Time)) {
                    return Problem(400, "Invalid trending time");
                }
                if (Categories != default && !await __SocialCategoryManagement.IsExistingCategories(CategoriesArr)) {
                    return Problem(400, "Invalid categories not exists.");
                }
                if (Tags != default && !await __SocialTagManagement.IsExistsTags(TagsArr)) {
                    return Problem(400, "Invalid tags not exists.");
                }
                #endregion

                #region Get posts
                var (Posts, TotalSize, Error) = await __SocialPostManagement
                    .GetTrendingPosts(
                        IsValidSession ? Session.UserId : default,
                        Time,
                        Start,
                        Size,
                        SearchTerm,
                        TagsArr,
                        CategoriesArr
                    );
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetTrendingPosts failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, $"Invalid request params start: { Start }. Total size is { TotalSize }");
                }
                #endregion

                var Ret = new List<JObject>();
                Posts.ForEach(e => {
                    var Obj = e.GetPublicShortJsonObject();
                    if (IsValidSession) {
                        Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    }
                    Ret.Add(Obj);
                });

                return Ok(200, "OK", new JObject(){
                    { "posts",      Utils.ObjectToJsonToken(Ret) },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
