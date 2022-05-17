using Common;
using CoreApi.Common;
using CoreApi.Models.ModifyModels;
using CoreApi.Services;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        public ModifyPostController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "ModifyPost";
        }

        /// <summary>
        /// Get social user by header session_token
        /// </summary>
        /// <returns><b>Social user of session_token</b></returns>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="__SocialPostManagement"></param>
        /// <param name="__SocialCategoryManagement"></param>
        /// <param name="__SocialTagManagement"></param>
        /// <param name="__PostId"></param>
        /// <param name="__ModelData"></param>
        /// <param name="SessionToken"></param>
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
        public async Task<IActionResult> ModifyPost([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                    [FromServices] SocialPostManagement         __SocialPostManagement,
                                                    [FromServices] SocialCategoryManagement     __SocialCategoryManagement,
                                                    [FromServices] SocialTagManagement          __SocialTagManagement,
                                                    [FromRoute(Name = "post_id")] long          __PostId,
                                                    [FromBody] SocialPostModifyModel            __ModelData,
                                                    [FromHeader(Name = "session_token")] string SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialCategoryManagement.SetTraceId(TraceId);
            __SocialPostManagement.SetTraceId(TraceId);
            __SocialTagManagement.SetTraceId(TraceId);
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

                #region validate params
                AddLogParam("post_id", __PostId);
                if (__PostId <= 0) {
                    return Problem(400, "Invalid request.");
                }
                #endregion

                #region get post by id
                var (Post, Error) = await __SocialPostManagement.FindPostById(__PostId);
                if (Error != ErrorCodes.NO_ERROR || Post.Owner != Session.UserId) {
                    return Problem(404, "Not found post.");
                }
                #endregion

                #region validate post modify nodel
                if (__ModelData.categories != default && !await __SocialCategoryManagement.IsExistingCategories(__ModelData.categories)) {
                    return Problem(400, $"Category not exist.");
                }

                if (__ModelData.tags != default) {
                    var IsValidTags = false;
                    (IsValidTags, Error) = await __SocialTagManagement.IsValidTags(__ModelData.tags);
                    if (!IsValidTags) {
                        if (Error == ErrorCodes.INVALID_PARAMS) {
                            return Problem(400, "Invalid tags.");
                        }
                        throw new Exception($"IsValidTags Failed, ErrorCode: { Error }");
                    }
                }
                #endregion

                if (Post.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)) {
                    Error = await __SocialPostManagement.AddPendingContent(Post.Id, __ModelData);
                } else if (
                    Post.StatusStr      == EntityStatus.StatusTypeToString(StatusType.Private)
                    || Post.StatusStr   == EntityStatus.StatusTypeToString(StatusType.Pending)
                ) {
                    Error = await __SocialPostManagement.ModifyPostNotApproved(Post.Id, __ModelData);
                } else {
                    return Problem(400, $"Not allow modify post has '{ Post.StatusStr }'.");
                }

                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, "No change detected.");
                    }
                    if (Post.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)) {
                        throw new Exception($"AddPendingContent Failed, ErrorCode: { Error }");
                    } else {
                        throw new Exception($"ModifyPostNotApproved Failed, ErrorCode: { Error }");
                    }
                }

                var Ret = Post.GetJsonObject();
                Ret.Add("actions", Utils.ObjectToJsonToken(Post.GetActionByUser(Session.UserId)));

                return Ok(200, "OK", new JObject(){
                    { "post", Ret },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
