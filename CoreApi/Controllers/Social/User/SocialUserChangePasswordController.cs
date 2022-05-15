using Common;
using CoreApi.Common;
using CoreApi.Models;
using CoreApi.Services;
using CoreApi.Services.Background;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.User
{
    [ApiController]
    [Route("/api/user/changepassword")]
    public class SocialUserChangePasswordController : BaseController
    {
        public SocialUserChangePasswordController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "SocialUserChangePassword";
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status410Gone, Type = typeof(StatusCode410Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ChangePassword([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                        [FromServices] SocialUserManagement         __SocialUserManagement,
                                                        [FromBody] Models.ChangePasswordModel       __ModelData,
                                                        [FromHeader(Name = "session_token")] string SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionSocialUserManagement,
                                                                SessionToken,
                                                                new ErrorCodes[] { ErrorCodes.PASSWORD_IS_EXPIRED });
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Check new password policy
                var ErroMsg = __SocialUserManagement.ValidatePasswordWithPolicy(__ModelData.new_password);
                if (ErroMsg != string.Empty) {
                    LogWarning($"New user not match password policy, error: { ErroMsg }");
                    return Problem(400, ErroMsg);
                }
                #endregion

                #region Validate old password
                if (PasswordEncryptor.EncryptPassword(__ModelData.old_password, Session.User.Salt) != Session.User.Password) {
                    return Problem(400, "Incorrect old password.");
                }
                #endregion

                var Error = await __SocialUserManagement.ChangePassword(Session.UserId, __ModelData.new_password);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NO_CHANGE_DETECTED) {
                        LogWarning($"No change detected when change password, user_name: { Session.User.UserName }");
                        return Problem(400, "No change detected.");
                    }
                    throw new Exception($"ChangePassword Failed, ErrorCode: { Error }");
                }

                LogInformation($"ChangePassword successfully, user_name: { Session.User.UserName }");
                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }
    }
}
