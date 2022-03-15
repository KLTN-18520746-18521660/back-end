using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseAccess.Context;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CoreApi.Common;
using Common;
using System.Text;
// using System.Data.Entity;
using System.Diagnostics;
// using System.Text.Json;
using CoreApi.Services;
using Microsoft.AspNetCore.Http;

namespace CoreApi.Controllers.Social.Session
{
    [ApiController]
    [Route("/session")]
    public class GetUserBySessionSocialController : BaseController
    {
        #region Services
        private BaseConfig __BaseConfig;
        private SessionSocialUserManagement __SessionSocialUserManagement;
        #endregion

        #region Config Value
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetUserBySessionSocialController(
            BaseConfig _BaseConfig,
            SessionSocialUserManagement _SessionSocialUserManagement
        ) : base() {
            __BaseConfig = _BaseConfig;
            __SessionSocialUserManagement = _SessionSocialUserManagement;
            __ControllerName = "GetUserBySessionSocial";
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

        /// <summary>
        /// Get social user by header session_token
        /// </summary>
        /// <returns><b>Social user of session_token</b></returns>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header 'session_token'.
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> Social session of user.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Session not found.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="401">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="403">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Missing header session_token.</li>
        /// <li>Header session_token is invalid.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpGet("user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetUserBySessionAsync()
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            try {
                #region Get session token
                string sessionToken = "";
                if (!GetHeader(HEADER_KEYS.API_KEY, out sessionToken)) {
                    LogDebug($"Missing header authorization.");
                    return Problem(403, "Missing header authorization.");
                }

                if (!Utils.IsValidSessionToken(sessionToken)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionSocialUserManagement.FindSessionForUse(sessionToken, EXPIRY_TIME, EXTENSION_TIME);

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(400, "Session not found.");
                    }
                    if (error == ErrorCodes.SESSION_HAS_EXPIRED) {
                        LogInformation($"Session has expired, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(401, "Session has expired.");
                    }
                    if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogInformation($"User has been locked, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(423, "You have been locked.");
                    }
                    throw new Exception($"FindSessionForUse Failed. ErrorCode: { error }");
                }
                #endregion

                var user = session.User;
                LogInformation($"Get info user by apikey success, user_name: { user.UserName }");
                return Ok( new JObject(){
                    { "status", 200 },
                    { "user", user.GetJsonObject() },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
