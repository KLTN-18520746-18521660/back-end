using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Models.ModifyModels;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.User
{
    [ApiController]
    [Route("/api/user")]
    public class ModifyUserController : BaseController
    {
        public ModifyUserController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpPut("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ModifyUser([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                    [FromServices] SocialUserManagement         __SocialUserManagement,
                                                    [FromBody] SocialUserModifyModel            __ModelData,
                                                    [FromHeader(Name = "session_token")] string SessionToken)
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

                #region validate sepecific rule
                if (Session.User.VerifiedEmail && __ModelData.email != default) {
                    return Problem(400, "Can't change email have verified.");
                }
                #endregion

                var Error = await __SocialUserManagement.ModifyUser(Session.UserId, __ModelData);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, "No change detected.");
                    }
                    throw new Exception($"ModifyUser Failed, ErrorCode: { Error }");
                }

                SocialUser User = default;
                (User, Error)   = await __SocialUserManagement.FindUserById(Session.UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"FindUserById Failed, ErrorCode: { Error }"); 
                }

                var RetVal = User.GetJsonObject();
                return Ok(200, "OK", new JObject(){
                    { "user", RetVal },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
