using Common;
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

namespace CoreApi.Controllers.Social.Post
{
    [ApiController]
    [Route("/api/post")]
    public class GetNewPostsController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetNewPostsController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "GetNewPosts";
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

        [HttpGet("new")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetNewPosts([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                     [FromServices] SocialCategoryManagement __SocialCategoryManagement,
                                                     [FromServices] SocialUserManagement __SocialUserManagement,
                                                     [FromServices] SocialPostManagement __SocialPostManagement,
                                                     [FromServices] SocialTagManagement __SocialTagManagement,
                                                     [FromHeader] string session_token,
                                                     [FromQuery] int start = 0,
                                                     [FromQuery] int size = 20,
                                                     [FromQuery] string search_term = default,
                                                     [FromQuery] Models.OrderModel orders = default,
                                                     [FromQuery] string[] tags = default,
                                                     [FromQuery] string[] categories = default)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialCategoryManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            __SocialPostManagement.SetTraceId(TraceId);
            __SocialTagManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (categories != default && !await __SocialCategoryManagement.IsExistingCategories(categories)) {
                    return Problem(400, "Invalid categories not exists.");
                }
                if (tags != default && !await __SocialTagManagement.IsExistsTags(tags)) {
                    return Problem(400, "Invalid tags not exists.");
                }
                var combineOrders = orders.GetOrders();
                var paramsAllowInOrder = __SocialPostManagement.GetAllowOrderFields(GetPostAction.GetNewPosts);
                foreach (var it in combineOrders) {
                    if (!paramsAllowInOrder.Contains(it.Item1)) {
                        return Problem(400, $"Not allow order field: { it.Item1 }.");
                    }
                }
                #endregion

                bool IsValidSession = false;
                #region Get session token
                if (session_token != default) {
                    IsValidSession = CommonValidate.IsValidSessionToken(session_token);
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                if (IsValidSession) {
                    (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                    if (error != ErrorCodes.NO_ERROR) {
                        IsValidSession = false;
                    }
                }
                #endregion

                #region Get posts
                List<SocialPost> posts = default;
                int totalSize = default;
                (posts, totalSize, error) = await __SocialPostManagement
                    .GetNewPosts(
                        start,
                        size,
                        search_term,
                        combineOrders,
                        tags,
                        categories
                    );
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetNewPosts failed, ErrorCode: { error }");
                }
                #endregion

                #region Validate params: start, size, total_size
                if (totalSize != 0 && start >= totalSize) {
                    LogWarning($"Invalid request params for get posts, start: { start }, size: { size }, search_term: { search_term }, total_size: { totalSize }");
                    return Problem(400, $"Invalid request params start: { start }. Total size is { totalSize }");
                }
                #endregion

                var ret = new List<JObject>();
                posts.ForEach(e => {
                    var obj = e.GetPublicShortJsonObject();
                    if (IsValidSession) {
                        obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionWithUser(session.UserId)));
                    }
                    ret.Add(obj);
                });

                return Ok(200, "OK", new JObject(){
                    { "posts", Utils.ObjectToJsonToken(ret) },
                    { "total_size", totalSize },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
