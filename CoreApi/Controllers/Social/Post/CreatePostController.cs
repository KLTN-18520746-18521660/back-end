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

namespace CoreApi.Controllers.Social.Post
{
    [ApiController]
    [Route("/api/post")]
    public class CreatePostController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public CreatePostController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "CreatePost";
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
        /// Create new post
        /// </summary>
        /// <returns><b>Social user of session_token</b></returns>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="__SocialPostManagement"></param>
        /// <param name="__SocialUserManagement"></param>
        /// <param name="__SocialCategoryManagement"></param>
        /// <param name="__SocialTagManagement"></param>
        /// <param name="__NotificationsManagement"></param>
        /// <param name="Parser"></param>
        /// <param name="session_token"></param>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header 'session_token'.
        /// - Body request have optional fields: 'thumbnail', 'short_content'
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
        /// <li>Mismatch user id.</li>
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
        /// <li>User not have full permission on right 'post'.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> CreatePost([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                    [FromServices] SocialPostManagement __SocialPostManagement,
                                                    [FromServices] SocialUserManagement __SocialUserManagement,
                                                    [FromServices] SocialCategoryManagement __SocialCategoryManagement,
                                                    [FromServices] SocialTagManagement __SocialTagManagement,
                                                    [FromServices] NotificationsManagement __NotificationsManagement,
                                                    [FromBody] ParserSocialPost Parser,
                                                    [FromHeader] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialPostManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            __SocialCategoryManagement.SetTraceId(TraceId);
            __SocialTagManagement.SetTraceId(TraceId);
            #endregion
            try {
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

                #region Validate post permission
                if (await __SocialUserManagement.HaveFullPermission(session.UserId, SOCIAL_RIGHTS.POST) == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    LogWarning($"User doesn't have permission for create new post, user_name: { session.User.UserName }");
                    return Problem(403, "User doesn't have permission for create new post. Contact admin for more detail.");
                }
                #endregion

                var post = new SocialPost();
                post.Parse(Parser, out var errMsg);
                post.Owner = session.UserId;
                // post.OwnerNavigation = session.User;
                if (errMsg != string.Empty) {
                    throw new Exception($"Parse social post model failed, error: { errMsg }");
                }

                #region validate post
                if (!await __SocialCategoryManagement.IsExistingCategories(Parser.categories)) {
                    return Problem(400, $"Category not exist.");
                }
                var isValidTags = false;
                var error = ErrorCodes.NO_ERROR;
                (isValidTags, error) = await __SocialTagManagement.IsValidTags(Parser.tags);
                if (!isValidTags) {
                    if (error == ErrorCodes.INVALID_PARAMS) {
                        return Problem(400, "Invalid tags.");
                    }
                    throw new Exception($"IsValidTags Failed, ErrorCode: { error }");
                }
                #endregion

                error = await __SocialPostManagement.AddNewPost(Parser, post, session.UserId);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"AddNewPost failed, ErrorCode: { error }");
                }

                LogInformation($"Add new post successfully, user_name: { session.User.UserName }, post_id: { post.Id }");
                await __NotificationsManagement.SendNotification(
                    NotificationType.ACTION_WITH_POST,
                    new PostNotificationModel(NotificationSenderAction.NEW_POST,
                                              session.UserId,
                                              default){
                        PostId = post.Id,
                    }
                );
                return Ok(201, "OK", new JObject(){
                    { "post_id", post.Id },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
