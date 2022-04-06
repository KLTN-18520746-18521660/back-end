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
    [Route("/signup")]
    public class SocialUserSignupController : BaseController
    {
        public SocialUserSignupController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "SocialUserSignup";
            LoadConfig();
        }

        /// <summary>
        /// Social user signup
        /// </summary>
        /// <returns><b>Return message ok</b></returns>
        /// <param name="__SocialUserManagement"></param>
        /// <param name="__EmailChannel"></param>
        /// <param name="parser"></param>
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
        public async Task<IActionResult> SocialUserSignup([FromServices] SocialUserManagement __SocialUserManagement,
                                                          [FromServices] Channel<EmailChannel> __EmailChannel,
                                                          [FromBody] ParserSocialUser parser)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Parse Social User
                SocialUser newUser = new SocialUser();
                string Error = "";
                if (!newUser.Parse(parser, out Error)) {
                    LogInformation(Error);
                    return Problem(400, "Bad request body.");
                }
                #endregion

                #region Check unique user_name, email
                bool username_existed = false, email_existed = false;
                var error = ErrorCodes.NO_ERROR;
                (username_existed, email_existed, error) = await __SocialUserManagement.IsUserExsiting(newUser.UserName, newUser.Email);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"IsUserExsiting Failed. ErrorCode: { error }");
                } else if (username_existed) {
                    LogDebug($"UserName have been used, user_name: { newUser.UserName }");
                    return Problem(400, "UserName have been used.");
                } else if (email_existed) {
                    LogDebug($"Email have been used, email: { newUser.Email }");
                    return Problem(400, "Email have been used.");
                }
                #endregion

                #region Add new social user
                error = await __SocialUserManagement.AddNewUser(newUser);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"AddNewSocialUser Failed. ErrorCode: { error }");
                }
                #endregion

                LogInformation($"Signup social user success, user_name: { newUser.UserName }");
                // var _ = __EmailSender.SendEmailUserSignUp(newUser.Id, __TraceId);
                await __EmailChannel.Writer.WriteAsync(new EmailChannel() {
                    TraceId = __TraceId,
                    Type = RequestToSendEmailType.UserSignup,
                    Data = new JObject() {
                        { "UserId", newUser.Id },
                    }
                });
                return Ok(201, "OK", new JObject(){
                    { "user_id", newUser.Id },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
