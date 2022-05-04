using Common;
using CoreApi.Common;
using CoreApi.Models.ModifyModels;
using CoreApi.Services;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Post
{
    [ApiController]
    [Route("/api/post")]
    public class ModifyPostController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public ModifyPostController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "ModifyPost";
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

        /// <summary>
        /// Get social user by header session_token
        /// </summary>
        /// <returns><b>Social user of session_token</b></returns>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="__SocialPostManagement"></param>
        /// <param name="__SocialCategoryManagement"></param>
        /// <param name="__SocialTagManagement"></param>
        /// <param name="ModelModify"></param>
        /// <param name="session_token"></param>
        /// <param name="post_id"></param>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header 'session_token'.
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> Social session of user.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Session not found.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="401">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="403">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Missing header session_token.</li>
        /// <li>Header session_token is invalid.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpPut("id/{post_id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ModifyPost([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                    [FromServices] SocialPostManagement __SocialPostManagement,
                                                    [FromServices] SocialCategoryManagement __SocialCategoryManagement,
                                                    [FromServices] SocialTagManagement __SocialTagManagement,
                                                    [FromBody] SocialPostModifyModel ModelModify,
                                                    [FromHeader] string session_token,
                                                    [FromRoute] long post_id)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region validate post id
                if (post_id <= 0) {
                    return Problem(400, "Invalid request.");
                }
                #endregion

                #region Get session token
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                    return Problem(401, "Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    return Problem(401, "Invalid header authorization.");
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

                #region get post by id
                SocialPost post = default;
                (post, error) = await __SocialPostManagement.FindPostById(post_id);
                if (error != ErrorCodes.NO_ERROR || post.Owner != session.UserId) {
                    return Problem(404, "Not found post.");
                }
                #endregion

                #region validate post
                if (ModelModify.categories != default && !await __SocialCategoryManagement.IsExistingCategories(ModelModify.categories)) {
                    return Problem(400, $"Category not exist.");
                }

                if (ModelModify.tags != default) {
                    var isValidTags = false;
                    (isValidTags, error) = await __SocialTagManagement.IsValidTags(ModelModify.tags);
                    if (!isValidTags) {
                        if (error == ErrorCodes.INVALID_PARAMS) {
                            return Problem(400, "Invalid tags.");
                        }
                        throw new Exception($"IsValidTags Failed, ErrorCode: { error }");
                    }
                }
                #endregion

                if (post.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)) {
                    error = await __SocialPostManagement.AddPendingContent(post.Id, ModelModify);
                } else if (
                    post.StatusStr == EntityStatus.StatusTypeToString(StatusType.Private)
                    || post.StatusStr == EntityStatus.StatusTypeToString(StatusType.Pending)
                ) {
                    error = await __SocialPostManagement.ModifyPostNotApproved(post.Id, ModelModify);
                } else {
                    return Problem(400, $"Not allow modify post has '{ post.StatusStr }'.");
                }

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, "No change detected.");
                    }
                    if (post.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)) {
                        throw new Exception($"AddPendingContent Failed, ErrorCode: { error }");
                    } else {
                        throw new Exception($"ModifyPostNotApproved Failed, ErrorCode: { error }");
                    }
                }

                var ret = post.GetJsonObject();
                ret.Add("actions", Utils.ObjectToJsonToken(post.GetActionByUser(session.UserId)));

                return Ok(200, "OK", new JObject(){
                    { "post", ret },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
