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

namespace CoreApi.Controllers.Social
{
    [ApiController]
    [Route("/api/search")]
    public class SearchController : BaseController
    {
        public int DEFAULT_SEARCH_SIZE = 6;
        public SearchController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> Search([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                [FromServices] SocialCategoryManagement __SocialCategoryManagement,
                                                [FromServices] SocialUserManagement __SocialUserManagement,
                                                [FromServices] SocialPostManagement __SocialPostManagement,
                                                [FromServices] SocialTagManagement __SocialTagManagement,
                                                [FromHeader(Name = "session_token")] string SessionToken,
                                                [FromQuery(Name = "search_term")] string SearchTerm = default)
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
                AddLogParam("search_term",  SearchTerm);
                if (SearchTerm == default || SearchTerm == string.Empty) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                #region Get posts
                var (Posts, TotalSizePosts, ErrorGetPosts) = await __SocialPostManagement
                    .SearchPosts(
                        IsValidSession ? Session.UserId : default,
                        0,
                        DEFAULT_SEARCH_SIZE,
                        SearchTerm,
                        default,
                        default,
                        default
                    );
                if (ErrorGetPosts != ErrorCodes.NO_ERROR) {
                    throw new Exception($"SearchPosts failed, ErrorCode: { ErrorGetPosts }");
                }

                var RetPosts = new List<JObject>();
                Posts.ForEach(e => {
                    var Obj = e.GetPublicShortJsonObject(IsValidSession ? Session.UserId : default);
                    if (IsValidSession) {
                        Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    }
                    RetPosts.Add(Obj);
                });
                #endregion

                #region Get users
                var (Users, TotalSizeUsers, ErrorGetUsers) = await __SocialUserManagement
                    .SearchUsers(
                        IsValidSession ? Session.UserId : default,
                        0,
                        DEFAULT_SEARCH_SIZE,
                        SearchTerm,
                        default
                    );
                if (ErrorGetUsers != ErrorCodes.NO_ERROR) {
                    throw new Exception($"SearchPosts failed, ErrorCode: { ErrorGetPosts }");
                }

                var RetUsers = new List<JObject>();
                Users.ForEach(e => {
                    var Obj = e.GetPublicJsonObject();
                    if (IsValidSession) {
                        if (Session.UserId == e.Id) {
                            Obj = e.GetJsonObject();
                        }
                        Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    }
                    RetUsers.Add(Obj);
                });
                #endregion

                #region Get tags
                var (Tags, TotalSizeTags) = await __SocialTagManagement
                    .SearchTags(
                        0,
                        DEFAULT_SEARCH_SIZE,
                        SearchTerm,
                        IsValidSession ? Session.UserId : default
                    );

                var RetTags = new List<JObject>();
                Tags.ForEach(e => {
                    var Obj = e.GetPublicJsonObject();
                    if (IsValidSession) {
                        Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    }
                    RetTags.Add(Obj);
                });
                #endregion

                #region Get categories
                var (Categories, TotalSizeCategories) = await __SocialCategoryManagement
                    .SearchCategories(
                        0,
                        DEFAULT_SEARCH_SIZE,
                        SearchTerm,
                        IsValidSession ? Session.UserId : default
                    );

                var RetCategories = new List<JObject>();
                Categories.ForEach(e => {
                    var Obj = e.GetPublicJsonObject();
                    if (IsValidSession) {
                        Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    }
                    RetCategories.Add(Obj);
                });
                #endregion

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { 
                        "search_post",
                        new JObject(){
                            { "posts", Utils.ObjectToJsonToken(RetPosts) },
                            { "total_size", TotalSizePosts },
                        }
                    },
                    { 
                        "search_user",
                        new JObject(){
                            { "users", Utils.ObjectToJsonToken(RetUsers) },
                            { "total_size", TotalSizeUsers },
                        }
                    },
                    { 
                        "search_tag",
                        new JObject(){
                            { "tags", Utils.ObjectToJsonToken(RetTags) },
                            { "total_size", TotalSizeTags },
                        }
                    },
                    { 
                        "search_category",
                        new JObject(){
                            { "categories", Utils.ObjectToJsonToken(RetCategories) },
                            { "total_size", TotalSizeCategories },
                        }
                    },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
