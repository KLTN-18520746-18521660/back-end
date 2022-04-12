using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Comment
{
    [ApiController]
    [Route("/comment")]
    public class DeleteCommentController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public DeleteCommentController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "DeleteComment";
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

        [HttpDelete("{comment_id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPostBySlug([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                       [FromServices] SocialCommentManagement __SocialCommentManagement,
                                                       [FromServices] SocialPostManagement __SocialPostManagement,
                                                       [FromRoute] long comment_id,
                                                       [FromHeader] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate slug
                if (comment_id == default || comment_id <= 0) {
                    return Problem(400, "Invalid request.");
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

                #region Get comment info
                SocialComment comment = default;
                (comment, error) = await __SocialCommentManagement.FindCommentById(comment_id);

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found comment.");
                    }

                    throw new Exception($"FindCommentById failed, ErrorCode: { error }");
                }
                #endregion

                if (comment.Owner != session.UserId) {
                    return Problem(403, "Not allow.");
                }

                error = await __SocialCommentManagement.DeleteComment(comment);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"DeleteComment failed, ErrorCode: { error }");
                }

                return Ok(200, "OK", new JObject(){
                    { "comment_id", comment.Id },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
