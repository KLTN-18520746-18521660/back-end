using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using CoreApi.Services.Background;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social
{
    [ApiController]
    [Route("/api/signup")]
    public class SocialUserSignupController : BaseController
    {
        public SocialUserSignupController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        /// <summary>
        /// Social user signup
        /// </summary>
        /// <returns><b>Return message ok</b></returns>
        /// <param name="__SocialUserManagement"></param>
        /// <param name="__EmailChannel"></param>
        /// <param name="__ParserModel"></param>
        ///
        /// <remarks>
        /// </remarks>
        ///
        /// <response code="201">
        /// <b>Success Case:</b> return message <q>Success.</q>.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Bad request body.</li>
        /// <li>Field 'user_name' or 'email' has been used.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SocialUserSignupSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> SocialUserSignup([FromServices] SocialUserManagement   __SocialUserManagement,
                                                          [FromServices] Channel<EmailChannel>  __EmailChannel,
                                                          [FromBody] ParserSocialUser           __ParserModel)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SocialUserManagement);
            #endregion
            try {
                #region Parse Social User
                SocialUser NewUser = new SocialUser();
                string ErrorParser = string.Empty;
                if (!NewUser.Parse(__ParserModel, out ErrorParser)) {
                    AddLogParam("error_parser", ErrorParser);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_BODY);
                }
                #endregion

                #region Check unique user_name, email
                var (UserNameExisted, EmailExisted, Error) = await __SocialUserManagement.IsUserExsiting(NewUser.UserName, NewUser.Email);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"IsUserExsiting failed. ErrorCode: { Error }");
                } else if (UserNameExisted) {
                    return Problem(400, RESPONSE_MESSAGES.USERNAME_HAS_BEEN_USED);
                } else if (EmailExisted) {
                    return Problem(400, RESPONSE_MESSAGES.EMAIL_HAS_BEEN_USED);
                }
                #endregion

                #region Check password policy
                var ErroMsg = __SocialUserManagement.ValidatePasswordWithPolicy(__ParserModel.password);
                if (ErroMsg != string.Empty) {
                    AddLogParam("error_valiate", ErroMsg);
                    return Problem(400, RESPONSE_MESSAGES.POLICY_PASSWORD_VIOLATION, new string[]{ ErroMsg });
                }
                #endregion

                #region Add new social user
                Error = await __SocialUserManagement.AddNewUser(NewUser);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"AddNewUser failed. ErrorCode: { Error }");
                }
                #endregion

                await __EmailChannel.Writer.WriteAsync(new EmailChannel() {
                    TraceId     = TraceId,
                    Type        = RequestToSendEmailType.UserSignup,
                    Data        = new JObject() {
                        { "UserId",         NewUser.Id },
                        { "IsAdminUser",    false },
                    }
                });

                return Ok(201, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "user_id", NewUser.Id },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
