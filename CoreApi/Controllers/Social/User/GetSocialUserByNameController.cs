using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.User
{
    [ApiController]
    [Route("/api/user")]
    public class GetSocialUserByNameController : BaseController
    {
        public GetSocialUserByNameController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "GetSocialUserByName";
        }

        [HttpGet("{user_name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetUserByUserName([FromServices] SessionSocialUserManagement   __SessionSocialUserManagement,
                                                           [FromServices] SocialUserManagement          __SocialUserManagement,
                                                           [FromRoute(Name = "user_name")] string       __UserName,
                                                           [FromHeader(Name = "session_token")] string  SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (__UserName == default || __UserName == string.Empty || __UserName.Length < 4) {
                    return Problem(400, "Invalid user_name.");
                }
                #endregion

                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
                var Session             = __Session as SessionSocialUser;
                #endregion

                var (User, Error) = await __SocialUserManagement.FindUser(__UserName, false);
                if (Error != ErrorCodes.NO_ERROR) {
                    return Problem(404, "Not found any user.");
                }

                var RetVal = (IsValidSession && Session.User.Id == User.Id) ? User.GetJsonObject() : User.GetPublicJsonObject();
                if (IsValidSession) {
                    RetVal.Add("actions", Utils.ObjectToJsonToken(User.GetActionByUser(Session.UserId)));
                }

                LogInformation($"Get info user by user_name success, user_name: { User.UserName }");
                return Ok(200, "OK", new JObject(){
                    { "user", RetVal },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }

        [HttpGet("{user_name}/statistic")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetStatisticUserByUserName([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                                    [FromServices] SocialUserManagement         __SocialUserManagement,
                                                                    [FromRoute(Name = "user_name")] string      __UserName,
                                                                    [FromHeader(Name = "session_token")] string SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (__UserName == default || __UserName == string.Empty || __UserName.Length < 4) {
                    return Problem(400, "Invalid user_name.");
                }
                #endregion

                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
                var Session             = __Session as SessionSocialUser;
                #endregion

                var (User, Error) = await __SocialUserManagement.FindUser(__UserName, false);
                if (Error != ErrorCodes.NO_ERROR) {
                    return Problem(404, "Not found any user.");
                }

                LogInformation($"Get statistic info user by user_name success, user_name: { User.UserName }");
                var RetVal = (IsValidSession && Session.User.Id == User.Id) ? User.GetStatisticInfo() : User.GetPublicStatisticInfo();
                return Ok(200, "OK", new JObject(){
                    { "user", RetVal },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }
    }
}
