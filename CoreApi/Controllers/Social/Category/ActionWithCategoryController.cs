using Common;
using CoreApi.Common.Base;
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

namespace CoreApi.Controllers.Social.Category
{
    [ApiController]
    [Route("/api/category")]
    public class ActionWithCategoryController : BaseController
    {
        protected readonly string[] ValidActions = new string[]{
            "follow",
            "unfollow",
        };

        public ActionWithCategoryController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpPost("{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ActionWithCategory([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                            [FromServices] SocialCategoryManagement     __SocialCategoryManagement,
                                                            [FromRoute(Name = "category")] string       __Category,
                                                            [FromQuery(Name = "action")] string         Action,
                                                            [FromHeader(Name = "session_token")] string SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionSocialUserManagement,
                __SocialCategoryManagement
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
                AddLogParam("category", __Category);
                AddLogParam("action",   Action);
                if (!__SocialCategoryManagement.IsValidCategory(__Category)) {
                    return Problem(400, "Invalid request.");
                }
                if (!ValidActions.Contains(Action)) {
                    return Problem(400, "Invalid params.");
                }
                #endregion

                var (FindCategory, Error) = await __SocialCategoryManagement.FindCategoryByName(__Category);

                if (Error != ErrorCodes.NO_ERROR && Error != ErrorCodes.USER_IS_NOT_OWNER) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found category.");
                    }

                    throw new Exception($"FindCategoryByName failed, ErrorCode: { Error }");
                }

                if (await __SocialCategoryManagement.IsContainsAction(FindCategory.Id, Session.UserId, Action)) {
                    return Problem(400, $"User already { Action } this category.");
                }

                switch (Action) {
                    case "follow":
                        Error = await __SocialCategoryManagement.Follow(FindCategory.Id, Session.UserId);
                        break;
                    case "unfollow":
                        Error = await __SocialCategoryManagement.UnFollow(FindCategory.Id, Session.UserId);
                        break;
                    default:
                        return Problem(400, "Invalid action.");
                }

                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"{ Action } category Failed, ErrorCode: { Error }");
                }

                return Ok(200, "OK");
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
