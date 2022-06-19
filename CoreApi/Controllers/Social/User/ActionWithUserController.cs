using Common;
using CoreApi.Common.Base;
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
using System.Collections.Generic;
using System.Linq;

namespace CoreApi.Controllers.Social.User
{
    [ApiController]
    [Route("/api/user")]
    public class ActionWithUserController : BaseController
    {
        protected readonly string[] ValidActions = new string[]{
            "follow",
            "unfollow",
        };

        public ActionWithUserController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpPost("{user_name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ActionWithUser([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                        [FromServices] SocialUserManagement         __SocialUserManagement,
                                                        [FromServices] NotificationsManagement      __NotificationsManagement,
                                                        [FromRoute(Name = "user_name")] string      __UserName,
                                                        [FromHeader(Name = "session_token")] string SessionToken,
                                                        [FromQuery(Name = "action")] string         Action)
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
                AddLogParam("des_user_name", __UserName);
                AddLogParam("action", Action);
                if (__UserName == default || __UserName.Trim() == string.Empty) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if (!ValidActions.Contains(Action)) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                #region Get user-des info
                var (UseDes, Error) = await __SocialUserManagement.FindUser(__UserName, false);

                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "user destination" });
                    }

                    throw new Exception($"FindUser failed, ErrorCode: { Error }, user_name: { __UserName }");
                }
                if (UseDes.Id == Session.UserId) {
                    return Problem(403, RESPONSE_MESSAGES.NOT_ALLOW_TO_DO, new string[]{ Action });
                }
                #endregion

                if (await __SocialUserManagement.IsContainsAction(UseDes.Id, Session.UserId, Action)) {
                    return Problem(400, RESPONSE_MESSAGES.ACTION_HAS_BEEN_TAKEN, new string[]{ Action });
                }
                NotificationSenderAction notificationAction = NotificationSenderAction.INVALID_ACTION;
                switch (Action) {
                    case "follow":
                        Error = await __SocialUserManagement.Follow(UseDes.Id, Session.UserId);
                        notificationAction = NotificationSenderAction.FOLLOW_USER;
                        break;
                    case "unfollow":
                        Error = await __SocialUserManagement.UnFollow(UseDes.Id, Session.UserId);
                        break;
                    default:
                        return Problem(400, RESPONSE_MESSAGES.INVALID_ACTION, new string[]{ Action });
                }

                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"{ Action } post failed, ErrorCode: { Error }");
                }

                if (notificationAction != NotificationSenderAction.INVALID_ACTION) {
                    await __NotificationsManagement.SendNotification(
                        NotificationType.ACTION_WITH_USER,
                        new UserNotificationModel(notificationAction,
                                                  Session.UserId,
                                                  default){
                            UserId = UseDes.Id
                        }
                    );
                }

                return Ok(200, RESPONSE_MESSAGES.OK);
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
