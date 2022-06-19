using Common;
using CoreApi.Common.Base;
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

namespace CoreApi.Controllers.Social.Notification
{
    [ApiController]
    [Route("/api/notification")]
    public class GetNotificationsByUserController : BaseController
    {
        public GetNotificationsByUserController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            // ControllerName = "GetNotificationsByUser";
        }

        /// <summary>
        /// Get all post attach to user
        /// </summary>
        /// <returns><b>Social user of session_token</b></returns>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="__SocialUserManagement"></param>
        /// <param name="__NotificationsManagement"></param>
        /// <param name="SessionToken"></param>
        /// <param name="Start"></param>
        /// <param name="Size"></param>
        /// <param name="Status"></param>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Header 'session_token' is optional.
        /// - If have session_token --> compare user is owner ?
        ///     - Is owner --> return full info of post (include post is pendding, reject, private)
        ///     - Else --> return info of public post (just post approve)
        /// - <i>Not allow search post of user have delete status.</i>
        /// - Must have query params for paging 'first', 'size'
        /// - Support query params 'status' (approve | pendding | reject | private) for filter
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
        [HttpGet("")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetNotificationsOfUser([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                                [FromServices] SocialUserManagement         __SocialUserManagement,
                                                                [FromServices] NotificationsManagement      __NotificationsManagement,
                                                                [FromHeader(Name = "session_token")] string SessionToken,
                                                                [FromQuery(Name = "start")] int             Start   = 0,
                                                                [FromQuery(Name = "size")] int              Size    = 20,
                                                                [FromQuery(Name = "status")] string         Status  = default)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SessionSocialUserManagement, __SocialUserManagement);
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

                #region Validate params
                AddLogParam("start", Start);
                AddLogParam("size", Size);
                AddLogParam("status", Status);
                var (StatusArr, ErrRetValidate) = ValidateStatusParams(Status, new StatusType[] { StatusType.Deleted });
                if (ErrRetValidate != default) {
                    return ErrRetValidate;
                }
                if (StatusArr == default) {
                    throw new Exception($"ValidateStatusParams failed.");
                }
                #endregion

                #region Get notifications
                var (Notifications, TotalSize) = await __NotificationsManagement
                    .GetNotifications(
                        Session.UserId,
                        Start,
                        Size,
                        StatusArr
                    );
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                var Ret = new List<JObject>();
                Notifications.ForEach(e => {
                    var Obj = e.GetPublicJsonObject();
                    Ret.Add(Obj);
                });

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "notifications", Utils.ObjectToJsonToken(Ret) },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
