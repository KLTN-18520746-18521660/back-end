using Common;
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
using System.Collections.Generic;
using System.Linq;

namespace CoreApi.Controllers.Social.Tag
{
    [ApiController]
    [Route("/api/tag")]
    public class ActionWithTagController : BaseController
    {
        protected readonly string[] ValidActions = new string[]{
            "follow",
            "unfollow",
        };

        public ActionWithTagController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "ActionWithTag";
        }

        [HttpPost("{tag}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ActionWithTag([FromServices] SessionSocialUserManagement   __SessionSocialUserManagement,
                                                       [FromServices] SocialTagManagement           __SocialTagManagement,
                                                       [FromRoute(Name = "tag")] string             __Tag,
                                                       [FromHeader(Name = "session_token")] string  SessionToken,
                                                       [FromQuery(Name = "action")] string          Action)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialTagManagement.SetTraceId(TraceId);
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
                AddLogParam("tag", __Tag);
                AddLogParam("action", Action);
                if (!__SocialTagManagement.IsValidTag(__Tag)) {
                    return Problem(400, "Invalid request.");
                }
                if (!ValidActions.Contains(Action)) {
                    return Problem(400, "Invalid params.");
                }
                #endregion

                #region find tag info
                var (FindTag, Error) = await __SocialTagManagement.FindTagByName(__Tag);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found tag");
                    }

                    throw new Exception($"FindTagByName failed, ErrorCode: { Error }");
                }
                #endregion

                if (await __SocialTagManagement.IsContainsAction(FindTag.Id, Session.UserId, Action)) {
                    return Problem(400, $"User already { Action } this tag.");
                }
                switch (Action) {
                    case "follow":
                        Error = await __SocialTagManagement.Follow(FindTag.Id, Session.UserId);
                        break;
                    case "unfollow":
                        Error = await __SocialTagManagement.UnFollow(FindTag.Id, Session.UserId);
                        break;
                    default:
                        return Problem(400, "Invalid action.");
                }

                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"{ Action } tag Failed, ErrorCode: { Error }, user_name { Session.User.UserName }");
                }

                return Ok(200, "OK");
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
