using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        public GetCommentByPostController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpGet("post/{post_slug}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetCommentByPost([FromServices] SessionSocialUserManagement        __SessionSocialUserManagement,
                                                          [FromServices] SocialCommentManagement            __SocialCommentManagement,
                                                          [FromServices] SocialPostManagement               __SocialPostManagement,
                                                          [FromRoute(Name = "post_slug")] string            __PostSlug,
                                                          [FromHeader(Name = "session_token")] string       SessionToken,
                                                          [FromQuery(Name = "parent_comment_id")] long?     ParentCommentId    = default,
                                                          [FromQuery(Name = "start")] int                   Start               = 0,
                                                          [FromQuery(Name = "size")] int                    Size                = 20,
                                                          [FromQuery(Name = "search_term")] string          SearchTerm          = default,
                                                          [FromQuery(Name = "status")] string               Status              = default,
                                                          [FromQuery] Models.OrderModel                     Orders              = default)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionSocialUserManagement,
                __SocialCommentManagement,
                __SocialPostManagement
            );
            #endregion
            try {
                #region Get config values
                var LimitSizeGetReplyComment = GetConfigValue<int>(CONFIG_KEY.API_GET_COMMENT_CONFIG, SUB_CONFIG_KEY.LIMIT_SIZE_GET_REPLY_COMMENT);
                #endregion

                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Validate params
                AddLogParam("post_slug", __PostSlug);
                IActionResult ErrRetValidate    = default;
                (string, bool)[] CombineOrders  = default;
                string[] StatusArr              = default;
                string[] AllowOrderParams       = __SocialCommentManagement.GetAllowOrderFields(GetCommentAction.GetCommentsAttachedToPost);
                if (__PostSlug == default || __PostSlug.Trim() == string.Empty) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if (ParentCommentId != default && ParentCommentId <= 0) {
                    AddLogParam("parent_comment_id", ParentCommentId);
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                (CombineOrders, ErrRetValidate) = ValidateOrderParams(Orders, AllowOrderParams);
                if (ErrRetValidate != default) {
                    return ErrRetValidate;
                }
                if (CombineOrders == default) {
                    throw new Exception($"ValidateOrderParams failed.");
                }
                (StatusArr, ErrRetValidate) = ValidateStatusParams(Status, new StatusType[] { StatusType.Deleted });
                if (ErrRetValidate != default) {
                    return ErrRetValidate;
                }
                if (StatusArr == default) {
                    throw new Exception($"ValidateStatusParams failed.");
                }
                #endregion

                var (Post, Error) = await __SocialPostManagement.FindPostBySlug(__PostSlug.Trim());

                if (Error != ErrorCodes.NO_ERROR && Error != ErrorCodes.USER_IS_NOT_OWNER) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "post" });
                    }
                    throw new Exception($"FindPostBySlug failed, ErrorCode: { Error }");
                }

                List<SocialComment> Comments    = default;
                int TotalSize                   = default;
                (Comments, TotalSize, Error) = await __SocialCommentManagement
                    .GetCommentsAttachedToPost(
                        Post.Id,
                        ParentCommentId,
                        Start,
                        Size,
                        SearchTerm,
                        StatusArr,
                        CombineOrders
                    );
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetCommentsAttachedToPost failed, ErrorCode: { Error }");
                }

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                var Ret = new List<JObject>();
                foreach (var Comment in Comments) {
                    var Obj = Comment.GetPublicJsonObject();
                    if (IsValidSession) {
                        Obj.Add("actions", Utils.ObjectToJsonToken(Comment.GetActionByUser(Session.UserId)));
                    }

                    #region Handle reply comments
                    List<SocialComment> ReplyComments = default;
                    int ReplyCommentTotalSize = default;
                    var ReplyRet = new List<JObject>();
                    (ReplyComments, ReplyCommentTotalSize, Error) = await __SocialCommentManagement
                        .GetCommentsAttachedToPost(
                            Post.Id,
                            Comment.Id,
                            0,
                            LimitSizeGetReplyComment,
                            SearchTerm,
                            StatusArr,
                            CombineOrders
                        );
                    ReplyComments.ForEach(e => {
                        var childObj = e.GetPublicJsonObject();
                        if (IsValidSession) {
                            childObj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                        }
                        ReplyRet.Add(childObj);
                    });
                    Obj.Add("reply_comments", new JObject(){
                        { "comments", Utils.ObjectToJsonToken(ReplyRet) },
                        { "total_size", ReplyCommentTotalSize },
                    });
                    #endregion

                    Ret.Add(Obj);
                }

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "comments",   Utils.ObjectToJsonToken(Ret) },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
