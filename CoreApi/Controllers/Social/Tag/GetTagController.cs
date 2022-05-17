using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Tag
{
    [ApiController]
    [Route("/api/tag")]
    public class GetTagController : BaseController
    {
        public GetTagController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            // ControllerName = "GetTag";
        }

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetTags([FromServices] SessionSocialUserManagement     __SessionSocialUserManagement,
                                                 [FromServices] SocialTagManagement             __SocialTagManagement,
                                                 [FromHeader(Name = "session_token")] string    SessionToken,
                                                 [FromQuery(Name = "start")] int                Start       = 0,
                                                 [FromQuery(Name = "size")] int                 Size        = 20,
                                                 [FromQuery(Name = "search_term")] string       SearchTerm  = default)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialTagManagement.SetTraceId(TraceId);
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
                    return Problem(400, "Bad request params.");
                }
                #endregion

                var (Tags, TotalSize)   = await __SocialTagManagement.GetTags(Start,
                                                                              Size,
                                                                              SearchTerm,
                                                                              IsValidSession ? Session.UserId : default);

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, $"Invalid request params start: { Start }. Total size is { TotalSize }");
                }
                #endregion

                var RetVal = new List<JObject>();
                Tags.ForEach(e => {
                    var Obj = e.GetPublicJsonObject();
                    if (IsValidSession) {
                        Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    }
                    RetVal.Add(Obj);
                });

                return Ok(200, "OK", new JObject(){
                    { "tags",       Utils.ObjectToJsonToken(RetVal) },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }

        [HttpGet("{tag}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetTagByName([FromServices] SessionSocialUserManagement    __SessionSocialUserManagement,
                                                      [FromServices] SocialTagManagement            __SocialTagManagement,
                                                      [FromRoute(Name = "tag")] string              __Tag,
                                                      [FromHeader(Name = "session_token")] string   SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialTagManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Validate params
                AddLogParam("tag", __Tag);
                if (!__SocialTagManagement.IsValidTag(__Tag)) {
                    return Problem(400, "Invalid tag.");
                }
                #endregion

                var (FindTag, Error) = await __SocialTagManagement.FindTagByName(__Tag, IsValidSession ? Session.UserId : default);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found tag.");
                    }
                    throw new Exception($"FindTagByName failed, ErrorCode: { Error }");
                }

                var ret = FindTag.GetPublicJsonObject();
                if (IsValidSession) {
                    ret.Add("actions", Utils.ObjectToJsonToken(FindTag.GetActionByUser(Session.UserId)));
                }

                return Ok(200, "OK", new JObject(){
                    { "tag", Utils.ObjectToJsonToken(ret) },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
