using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social
{
    [ApiController]
    [Route("/api/login")]
    public class SocialUserLoginController : BaseController
    {
        public SocialUserLoginController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        /// <summary>
        /// Social user login
        /// </summary>
        /// <param name="__SocialUserManagement"></param>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="__ModelData"></param>
        /// <returns><b>Return session_id</b></returns>
        ///
        /// <remarks>
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> return 'session_id' and 'user_id'.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Bad request body.</li>
        /// <li>User not found or incorrect password.</li>
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
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SocialUserLoginSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> SocialUserLogin([FromServices] SocialUserManagement        __SocialUserManagement,
                                                         [FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                         [FromBody] Models.LoginModel               __ModelData)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SocialUserManagement, __SessionSocialUserManagement);
            #endregion
            try {
                #region Get config values
                var LockTime                        = GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG,
                                                                          SUB_CONFIG_KEY.LOCK_TIME);
                var ExpiryTime                      = GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG,
                                                                          SUB_CONFIG_KEY.EXPIRY_TIME);
                var NumberOfTimesAllowLoginFailure  = GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG,
                                                                          SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_FAILURE);
                #endregion

                #region Find User
                var IsEmail         = CommonValidate.IsEmail(__ModelData.user_name.ToLower());
                var (User, Error)   = await __SocialUserManagement.FindUser(__ModelData.user_name, IsEmail);
                if (Error != ErrorCodes.NO_ERROR) {
                    return Problem(400, RESPONSE_MESSAGES.USER_NOT_FOUND_OR_INCORRECT_PASSWORD);
                }
                #endregion

                #region Check user is lock or not
                if (User.Status.Type == StatusType.Blocked) {
                    return Problem(423, RESPONSE_MESSAGES.USER_HAS_BEEN_LOCKED);
                }
                #endregion

                #region Compare password
                if (PasswordEncryptor.EncryptPassword(__ModelData.password, User.Salt) != User.Password) {
                    Error = await __SocialUserManagement.HandleLoginFail(User.Id, LockTime, NumberOfTimesAllowLoginFailure);
                    if (Error != ErrorCodes.NO_ERROR) {
                        throw new Exception($"HandleLoginFail failed. ErrorCode: { Error }");
                    }
                    return Problem(400, RESPONSE_MESSAGES.USER_NOT_FOUND_OR_INCORRECT_PASSWORD);
                }
                #endregion

                #region Create session
                var Data                    = __ModelData.data == default ? new JObject() : __ModelData.data;
                Data                        = GetRequestMetaData(Data);
                SessionSocialUser Session   = default;
                (Session, Error) = await __SessionSocialUserManagement.NewSession(User.Id, __ModelData.remember, Data);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"NewSession failed. ErrorCode: { Error }");
                }

                Error = await __SocialUserManagement.HandleLoginSuccess(User.Id);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"HandleLoginSuccess failed. ErrorCode: { Error }");
                }
                #endregion

                #region Set cookie header
                Response.Cookies.Append(SessionTokenHeaderKey,
                                        Session.SessionToken,
                                        GetCookieOptions(__ModelData.remember ? default : DateTime.UtcNow.AddMinutes(ExpiryTime)));
                #endregion

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "session_id",     Session.SessionToken },
                    { "user_id",        User.Id },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
