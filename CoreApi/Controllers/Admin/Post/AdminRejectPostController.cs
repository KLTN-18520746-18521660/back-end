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

namespace CoreApi.Controllers.Admin.Post
{
    [ApiController]
    [Route("/api/admin/post")]
    public class AdminRejectPostController : BaseController
    {
        public AdminRejectPostController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "AdminRejectPost";
            IsAdminController = true;
        }

        /// <summary>
        /// Reject social post
        /// </summary>
        /// <returns><b>Admin user of session_token</b></returns>
        /// <param name="__SessionAdminUserManagement"></param>
        /// <param name="__SocialPostManagement"></param>
        /// <param name="__AdminUserManagement"></param>
        /// <param name="__NotificationsManagement"></param>
        /// <param name="__PostId"></param>
        /// <param name="SessionToken"></param>
        /// <param name="RejectPendingContent"></param>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header 'session_token_admin'.
        /// - Query param: 'reject_pending_content' set true when reject the pending content of post.
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> Message 'OK'.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Invalid params post_id</li>
        /// <li>Not allow reject post</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="401">
        /// <b>Error case <i>(Server auto send response with will clear cookie 'session_token_admin')</i>, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
        /// <li>Session not found.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="403">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>User doesn't have permission to reject post.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="404">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Not found post.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="423">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>User have been locked.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpPost("reject/{post_id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StatusCode200OKExamples))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> RejectPost([FromServices] SessionAdminUserManagement           __SessionAdminUserManagement,
                                                    [FromServices] SocialPostManagement                 __SocialPostManagement,
                                                    [FromServices] AdminUserManagement                  __AdminUserManagement,
                                                    [FromServices] NotificationsManagement              __NotificationsManagement,
                                                    [FromRoute(Name = "post_id")] long                  __PostId,
                                                    [FromHeader(Name = "session_token_admin")] string   SessionToken,
                                                    [FromQuery(Name = "reject_pending_content")] bool   RejectPendingContent = false)
        {
            #region Set TraceId for services
            __SessionAdminUserManagement.SetTraceId(TraceId);
            __SocialPostManagement.SetTraceId(TraceId);
            __AdminUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionAdminUserManagement, SessionToken);
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session             = __Session as SessionAdminUser;
                #endregion

                #region Validate params
                if (__PostId <= 0) {
                    return Problem(400, "Invalid params.");
                }
                #endregion

                #region Check Permission
                var Error = __AdminUserManagement.HaveFullPermission(Session.User.Rights, ADMIN_RIGHTS.POST);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    LogWarning($"User doesn't have permission to approve social post, user_name: { Session.User.UserName }");
                    return Problem(403, "User doesn't have permission to approve social post.");
                }
                #endregion

                #region Get post info
                SocialPost Post = default;
                (Post, Error) = await __SocialPostManagement.FindPostById(__PostId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        LogWarning($"Not found social post, post_id: { __PostId }");
                        return Problem(404, "Not found post.");
                    }
                    throw new Exception($"FindPostById failed. Post_id: { __PostId }, ErrorCode: { Error} ");
                }
                if (RejectPendingContent == true && (Post.Status.Type != StatusType.Approved || Post.PendingContent == default)) {
                    LogWarning($"Reject modify post but post haven't pending content, post_status: { Post.StatusStr }");
                    return Problem(400, "Not allow to reject modify post (post not have 'pending_content').");
                }
                if (__SocialPostManagement.ValidateChangeStatusAction(Post.Status.Type, StatusType.Rejected) != ErrorCodes.NO_ERROR) {
                    LogWarning($"Not allow change status post, old_status: { Post.StatusStr }, new_status: Approved");
                    return Problem(400, "Not allow to reject post.");
                }
                #endregion

                Error = await __SocialPostManagement.RejectPost(Post.Id, Session.UserId, RejectPendingContent);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"RejectPost Failed, ErrorCode: { Error }");
                }

                await __NotificationsManagement.SendNotification(
                    NotificationType.ACTION_WITH_POST,
                    new PostNotificationModel(RejectPendingContent
                                                ? NotificationSenderAction.REJECT_MODIFY_POST
                                                : NotificationSenderAction.REJECT_POST,
                                              default,
                                              Session.UserId){
                        PostId = Post.Id,
                    }
                );

                LogInformation($"RejectPost success, post_id: { __PostId }, user_id: { Session.UserId }");
                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }
    }
}
