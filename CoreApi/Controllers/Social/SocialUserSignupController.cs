using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social
{
    [ApiController]
    [Route("/signup")]
    public class SocialUserSignupController : BaseController
    {
        #region Services
        private SocialUserManagement __SocialUserManagement;
        #endregion

        public SocialUserSignupController(
            SocialUserManagement _SocialUserManagement
        ) : base() {
            __SocialUserManagement = _SocialUserManagement;
            __ControllerName = "SocialUserSignup";
            LoadConfig();
        }

        /// <summary>
        /// Social user signup
        /// </summary>
        /// <returns><b>Return message ok</b></returns>
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
        public async Task<IActionResult> SocialUserSignupAsync(ParserSocialUser parser)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
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
                SocialUser tmpUser = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (tmpUser, error) = await __SocialUserManagement.FindUser(newUser.UserName, false);
                if (error == ErrorCodes.NO_ERROR) {
                    LogDebug($"UserName have been used, user_name: { newUser.UserName }");
                    return Problem(400, "UserName have been used.");
                }
                (tmpUser, error) = await __SocialUserManagement.FindUser(newUser.Email, true);
                if (error == ErrorCodes.NO_ERROR) {
                    LogDebug($"Email have been used, user_name: { newUser.Email }");
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
                return Ok(new JObject(){
                    { "status", 201 },
                    { "message", "Success." },
                    { "user_id", newUser.Id },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
