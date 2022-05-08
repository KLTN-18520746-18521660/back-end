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
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        protected readonly string[] ValidActions = new string[]{
            "follow",
            "unfollow",
        };

        public ActionWithTagController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "ActionWithTag";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = string.Empty;
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.ToString());
                if (Error != e.Message && Error != string.Empty) {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value failed, message: { msg }");
            }
        }

        [HttpPost("{tag}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ActionWithTag([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                       [FromServices] SocialTagManagement __SocialTagManagement,
                                                       [FromRoute] string tag,
                                                       [FromQuery] string action,
                                                       [FromHeader] string session_token)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialTagManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (!__SocialTagManagement.IsValidTag(tag)) {
                    return Problem(400, "Invalid request.");
                }
                if (!ValidActions.Contains(action)) {
                    return Problem(400, "Invalid params.");
                }
                #endregion

                #region Get session
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                var (__session, errRet) = await GetSessionToken(__SessionSocialUserManagement, EXPIRY_TIME, EXTENSION_TIME, session_token);
                if (errRet != default) {
                    return errRet;
                }
                if (__session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var session = __session as SessionSocialUser;
                #endregion

                #region find tag info
                SocialTag findTag       = default;
                var error               = ErrorCodes.NO_ERROR;
                (findTag, error) = await __SocialTagManagement.FindTagByName(tag);
                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found tag");
                    }

                    throw new Exception($"FindTagByName failed, ErrorCode: { error }");
                }
                #endregion

                if (await __SocialTagManagement.IsContainsAction(findTag.Id, session.UserId, action)) {
                    return Problem(400, $"User already { action } this tag.");
                }
                switch (action) {
                    case "follow":
                        error = await __SocialTagManagement.Follow(findTag.Id, session.UserId);
                        break;
                    case "unfollow":
                        error = await __SocialTagManagement.UnFollow(findTag.Id, session.UserId);
                        break;
                    default:
                        return Problem(400, "Invalid action.");
                }

                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"{ action } tag Failed, ErrorCode: { error }");
                }

                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
