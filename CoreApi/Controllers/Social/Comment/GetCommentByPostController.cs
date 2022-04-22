using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Comment
{
    [ApiController]
    [Route("/api/comment")]
    public class GetCommentByPostController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetCommentByPostController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "GetCommentByPost";
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

        [HttpGet("post/{post_slug}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetCommentByPost([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                          [FromServices] SocialCommentManagement __SocialCommentManagement,
                                                          [FromServices] SocialPostManagement __SocialPostManagement,
                                                          [FromRoute] string post_slug,
                                                          [FromHeader] string session_token,
                                                          [FromQuery] long? parrent_comment_id = default,
                                                          [FromQuery] int start = 0,
                                                          [FromQuery] int size = 20,
                                                          [FromQuery] string search_term = default,
                                                          [FromQuery] string[] status = default,
                                                          [FromQuery] Models.OrderModel orders = default)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialCommentManagement.SetTraceId(TraceId);
            __SocialPostManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (post_slug == default || post_slug.Trim() == string.Empty) {
                    return Problem(400, "Invalid request.");
                }
                var combineOrders = orders.GetOrders();
                var paramsAllowInOrder = __SocialCommentManagement.GetAllowOrderFields(GetCommentAction.GetCommentsAttachedToPost);
                foreach (var it in combineOrders) {
                    if (!paramsAllowInOrder.Contains(it.Item1)) {
                        return Problem(400, $"Not allow order field: { it.Item1 }.");
                    }
                }
                if (status != default) {
                    foreach (var statusStr in status) {
                        var statusInt = BaseStatus.StatusFromString(statusStr, EntityStatus.SocialCommentStatus);
                        if (statusInt == BaseStatus.InvalidStatus ||
                            statusInt == SocialCommentStatus.Deleted) {
                            return Problem(400, $"Invalid status: { statusStr }.");
                        }
                    }
                }
                if (parrent_comment_id != default && parrent_comment_id <= 0) {
                    return Problem(400, "Invalid parrent_comment_id.");
                }
                #endregion

                bool IsValidSession = false;
                #region Validate slug
                if (post_slug == default || post_slug.Trim() == string.Empty) {
                    return Problem(400, "Invalid request.");
                }
                #endregion

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

                SocialPost post = default;
                (post, error) = await __SocialPostManagement.FindPostBySlug(post_slug.Trim());

                if (error != ErrorCodes.NO_ERROR && error != ErrorCodes.USER_IS_NOT_OWNER) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found post.");
                    }

                    throw new Exception($"FindPostBySlug failed, ErrorCode: { error }");
                }

                List<SocialComment> comments = default;
                int totalSize = default;
                (comments, totalSize, error) = await __SocialCommentManagement
                    .GetCommentsAttachedToPost(
                        post.Id,
                        parrent_comment_id,
                        start,
                        size,
                        search_term,
                        status,
                        combineOrders
                    );
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetCommentsAttachedToPost failed, ErrorCode: { error }");
                }

                #region Validate params: start, size, total_size
                if (totalSize != 0 && start >= totalSize) {
                    LogWarning($"Invalid request params for get comments, start: { start }, size: { size }, search_term: { search_term }, total_size: { totalSize }");
                    return Problem(400, $"Invalid request params start: { start }. Total size is { totalSize }");
                }
                #endregion

                var ret = new List<JObject>();
                foreach (var comment in comments) {
                    var obj = comment.GetPublicJsonObject();
                    if (IsValidSession) {
                        obj.Add("actions", Utils.ObjectToJsonToken(comment.GetActionWithUser(session.UserId)));
                    }
                    #region Handle reply comments
                    List<SocialComment> replyComments = default;
                    int replyCommentTotalSize = default;
                    var replyRet = new List<JObject>();
                    (replyComments, replyCommentTotalSize, error) = await __SocialCommentManagement
                        .GetCommentsAttachedToPost(
                            post.Id,
                            comment.Id,
                            0,
                            2,
                            search_term,
                            status,
                            combineOrders
                        );
                    replyComments.ForEach(e => {
                        var childObj = e.GetPublicJsonObject();
                        if (IsValidSession) {
                            childObj.Add("actions", Utils.ObjectToJsonToken(e.GetActionWithUser(session.UserId)));
                        }
                        replyRet.Add(childObj);
                    });
                    obj.Add("reply_comments", new JObject(){
                        { "comments", Utils.ObjectToJsonToken(replyRet) },
                        { "total_size", replyCommentTotalSize },
                    });
                    #endregion
                    ret.Add(obj);
                }

                return Ok(200, "OK", new JObject(){
                    { "comments", Utils.ObjectToJsonToken(ret) },
                    { "total_size", totalSize },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
