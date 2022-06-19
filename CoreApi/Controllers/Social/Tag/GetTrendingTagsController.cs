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

namespace CoreApi.Controllers.Social.Tag
{
    [ApiController]
    [Route("/api/tag")]
    public class GetTrendingTagsController : BaseController
    {
        private int[] AllowTimeTrending = new int[]{
            -1,     // all time
            7,      // week
            30,     // month
            180,    // 6 month
        };

        public GetTrendingTagsController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpGet("trending")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetTrendingPosts([FromServices] SessionSocialUserManagement    __SessionSocialUserManagement,
                                                          [FromServices] SocialUserManagement           __SocialUserManagement,
                                                          [FromServices] SocialTagManagement            __SocialTagManagement,
                                                          [FromHeader(Name = "session_token")] string   SessionToken,
                                                          [FromQuery(Name = "time")] int                Time        = -1,
                                                          [FromQuery(Name = "start")] int               Start       = 0,
                                                          [FromQuery(Name = "size")] int                Size        = 20,
                                                          [FromQuery(Name = "search_term")] string      SearchTerm  = default)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionSocialUserManagement,
                __SocialUserManagement,
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
                if (Start < 0 || Size < 1) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if (!AllowTimeTrending.Contains(Time)) {
                    AddLogParam("time", Time);
                    AddLogParam("allow_times", AllowTimeTrending);
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                #region Get tags
                var (Tags, TotalSize, Error) = await __SocialTagManagement
                    .GetTrendingTags(
                        Time,
                        Start,
                        Size,
                        SearchTerm
                    );
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetTrendingPosts failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                var Ret = new List<JObject>();
                Tags.ForEach(e => {
                    var Obj = e.GetPublicJsonObject();
                    if (IsValidSession) {
                        Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    }
                    Ret.Add(Obj);
                });

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "tags",      Utils.ObjectToJsonToken(Ret) },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
