using Common;
using CoreApi.Common;
using CoreApi.Services;
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
    [Route("/post")]
    public class GetPostsByUserNameController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetPostsByUserNameController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "GetPostsByUserName";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.ToString());
                if (Error != e.Message && Error != "") {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value failed, message: { msg }");
            }
        }

        /// <summary>
        /// Get all post attach to user
        /// If have session_token --> compare user is owner ?
        ///     - Is owner --> return full info of post (include post is pendding, reject, private)
        ///     - Else --> return info of public post (just post approve)
        /// <i>Not allow search post of user have delete status.</i>
        /// Must have query params for paging 'first', 'size'
        /// Support query params 'status' (approve | pendding | reject | private) for filter
        /// </summary>
        /// <returns><b>Social user of session_token</b></returns>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="__SocialPostManagement"></param>
        /// <param name="user_name"></param>
        /// <param name="session_token"></param>
        /// <param name="first"></param>
        /// <param name="size"></param>
        /// <param name="search_term"></param>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Header 'session_token' is optional.
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
        /// <li>Invalid slug.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpGet("/user/{user_name}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPostsByUserName([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                            [FromServices] SocialPostManagement __SocialPostManagement,
                                                            [FromRoute] string user_name,
                                                            [FromHeader] string session_token,
                                                            [FromQuery] int first = 0,
                                                            [FromQuery] int size = 20,
                                                            [FromQuery] string search_term = null)
        {
            //////////////////////
            return Problem(500, "Not implement.");
            //////////////////////
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                // bool IsValidSession = false;
                // bool IsOwner = false;
                // #region Validate params
                // if (post_slug == default || post_slug.Trim() == string.Empty) {
                //     return Problem(400, "Invalid request.");
                // }
                // #endregion

                // #region Get session token
                // if (session_token != null) {
                //     IsValidSession = !Utils.IsValidSessionToken(session_token);
                // }
                // #endregion

                // #region Find session for use
                // SessionSocialUser session = null;
                // ErrorCodes error = ErrorCodes.NO_ERROR;
                // if (IsValidSession) {
                //     (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                //     if (error == ErrorCodes.NO_ERROR) {
                //         IsValidSession = true;
                //     }
                // }
                // #endregion

                // SocialPost post = default;
                // if (IsValidSession) {
                //     (post, error) = await __SocialPostManagement.FindPostBySlug(post_slug.Trim(), session.UserId);
                // } else {
                //     (post, error) = await __SocialPostManagement.FindPostBySlug(post_slug.Trim());
                // }
                // if (error != ErrorCodes.NO_ERROR) {
                //     if (error == ErrorCodes.NOT_FOUND) {
                //         return Problem(404, "Not found any post.");
                //     }
                //     throw new Exception($"FindPostBySlug failed, ErrorCode: { error }");
                // }

                // return Ok( new JObject(){
                //     { "status", 200 },
                //     { "post", post.GetJsonObject() },
                // });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
