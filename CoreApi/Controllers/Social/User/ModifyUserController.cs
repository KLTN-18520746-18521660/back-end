using Common;
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
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public ModifyUserController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "ModifyUser";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = string.Empty;
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.ToString());
                if (Error != e.Message && Error != string.Empty) {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value failed, message: { msg }");
            }
        }

        [HttpPut("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ModifyUser([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                    [FromServices] SocialUserManagement __SocialUserManagement,
                                                    [FromHeader] string session_token,
                                                    [FromBody] SocialUserModifyModel modelData)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session token
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                    return Problem(403, "Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { session_token.Substring(0, 15) }");
                        return Problem(401, "Session not found.");
                    }
                    if (error == ErrorCodes.SESSION_HAS_EXPIRED) {
                        LogInformation($"Session has expired, session_token: { session_token.Substring(0, 15) }");
                        return Problem(401, "Session has expired.");
                    }
                    if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogWarning($"User has been locked, session_token: { session_token.Substring(0, 15) }");
                        return Problem(423, "You have been locked.");
                    }
                    throw new Exception($"FindSessionForUse Failed. ErrorCode: { error }");
                }
                #endregion

                #region validate sepecific rule
                if (session.User.VerifiedEmail && modelData.email != default) {
                    return Problem(400, "Can't change email have verified.");
                }
                #endregion

                error = await __SocialUserManagement.ModifyUser(session.UserId, modelData);
                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, "No change detected.");
                    }
                    throw new Exception($"ModifyUser Failed, ErrorCode: { error }");
                }

                SocialUser user = default;
                (user, error) = await __SocialUserManagement.FindUserById(session.UserId);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"FindUserById Failed, ErrorCode: { error }"); 
                }

                var ret = user.GetJsonObject();
                return Ok(200, "OK", new JObject(){
                    { "user", ret },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
