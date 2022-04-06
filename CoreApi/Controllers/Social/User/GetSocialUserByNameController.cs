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

namespace CoreApi.Controllers.Social.Session
{
    [ApiController]
    [Route("/user")]
    public class GetSocialUserByNameController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetSocialUserByNameController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "GetSocialUserByName";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.ToString());
                if (Error != e.Message && Error != "") {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value failed, message: { msg }");
            }
        }

        [HttpGet("{user_name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetUserByUserName([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                           [FromServices] SocialUserManagement __SocialUserManagement,
                                                           [FromHeader] string session_token,
                                                           [FromRoute] string user_name)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                bool isSessionInvalid = false;
                #region Get session token
                if (session_token == null) {
                    LogDebug($"Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    LogDebug("Invalid header authorization.");
                }
                #endregion

                #region Validate user_name
                if (user_name == null || user_name == string.Empty || user_name.Length < 4) {
                    return Problem(400, "Invalid user_name.");
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                if (isSessionInvalid) {
                    (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                    if (error != ErrorCodes.NO_ERROR) {
                        isSessionInvalid = false;
                    }
                }
                #endregion

                SocialUser user = null;
                (user, error) = await __SocialUserManagement.FindUser(user_name, false);
                if (error != ErrorCodes.NO_ERROR) {
                    return Problem(404, "Not found any user.");
                }
                LogInformation($"Get info user by user_name success, user_name: { user.UserName }");

                var ret = (session != null && session.User.Id == user.Id) ? user.GetJsonObject() : user.GetPublicJsonObject();

                return Ok( new JObject(){
                    { "status", 200 },
                    { "message", "OK" },
                    { "data", new JObject(){
                        { "user", ret },
                    }},
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}