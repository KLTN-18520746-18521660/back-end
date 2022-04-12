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

namespace CoreApi.Controllers.Social.Post
{
    [ApiController]
    [Route("/post")]
    public class ActionWithPostController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        protected readonly string[] ValidActions = new string[]{
            "like",
            "unlike",
            "dislike",
            "undislike",
            "save",
            "unsave",
            "follow",
            "unfollow",
        };

        public ActionWithPostController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "ActionWithPost";
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

        [HttpPost("{post_slug}/{action}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ActionPost([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                          [FromServices] SocialPostManagement __SocialPostManagement,
                                                          [FromRoute] string post_slug,
                                                          [FromRoute] string action,
                                                          [FromHeader] string session_token)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialPostManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (post_slug == default || post_slug.Trim() == string.Empty) {
                    return Problem(400, "Invalid request.");
                }
                if (!ValidActions.Contains(action)) {
                    return Problem(400, "Invalid params.");
                }
                #endregion

                #region Get session token
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                    return Problem(403, "Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { session_token.Substring(0, 15) }");
                        return Problem(401, "Session not found.");
                    }
                    if (error == ErrorCodes.SESSION_HAS_EXPIRED) {
                        LogInformation($"Session has expired, session_token: { session_token.Substring(0, 15) }");
                        return Problem(401, "Session has expired.");
                    }
                    if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogWarning($"User has been locked, session_token: { session_token.Substring(0, 15) }");
                        return Problem(423, "You have been locked.");
                    }
                    throw new Exception($"FindSessionForUse Failed. ErrorCode: { error }");
                }
                #endregion

                #region Get post info
                SocialPost post = default;
                (post, error) = await __SocialPostManagement.FindPostBySlug(post_slug.Trim(), session.UserId);

                if (error != ErrorCodes.NO_ERROR && error != ErrorCodes.USER_IS_NOT_OWNER) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found post.");
                    }

                    throw new Exception($"FindPostBySlug failed, ErrorCode: { error }");
                }
                #endregion

                switch (action) {
                    case "like":
                        error = await __SocialPostManagement.Like(post.Id, session.UserId);
                        break;
                    case "unlike":
                        error = await __SocialPostManagement.UnLike(post.Id, session.UserId);
                        break;
                    case "dislike":
                        error = await __SocialPostManagement.DisLike(post.Id, session.UserId);
                        break;
                    case "undislike":
                        error = await __SocialPostManagement.UnDisLike(post.Id, session.UserId);
                        break;
                    case "save":
                        error = await __SocialPostManagement.Save(post.Id, session.UserId);
                        break;
                    case "unsave":
                        error = await __SocialPostManagement.UnSave(post.Id, session.UserId);
                        break;
                    case "follow":
                        error = await __SocialPostManagement.Follow(post.Id, session.UserId);
                        break;
                    case "unfollow":
                        error = await __SocialPostManagement.UnFollow(post.Id, session.UserId);
                        break;
                    default:
                        return Problem(400, "Invalid action.");
                }

                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"{ action } post Failed, ErrorCode: { error }");
                }

                return Ok(200, "Ok");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
