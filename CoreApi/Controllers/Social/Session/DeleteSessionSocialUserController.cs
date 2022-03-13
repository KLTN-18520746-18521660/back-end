
// using System.Data.Entity;
// using System.Text.Json;
using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Session
{
    [ApiController]
    [Route("/session")]
    public class DeleteSessionSocialUserController : BaseController
    {
        #region Services
        private BaseConfig __BaseConfig;
        private SessionSocialUserManagement __SessionSocialUserManagement;
        #endregion

        #region Config Value
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minutes
        #endregion

        public DeleteSessionSocialUserController(
            BaseConfig _BaseConfig,
            SessionSocialUserManagement _SessionSocialUserManagement
        ) : base() {
            __BaseConfig = _BaseConfig;
            __SessionSocialUserManagement = _SessionSocialUserManagement;
            __ControllerName = "DeleteSessionSocialUser";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                EXTENSION_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, "extension_time", out Error);
                EXPIRY_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, "expiry_time", out Error);
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
        /// Delete social session
        /// </summary>
        /// <param name="session_token"></param>
        /// <returns><b>Message ok</b></returns>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header 'session_token'.
        /// - Need path param 'session_token' for delete.
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> return message <q>Success</q>.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Bad request params, header.</li>
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
        /// <response code="404">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Not found session to delete.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i> See server log for detail.</i>
        /// </response>
        [HttpDelete("{session_token}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeleteSessionSocialUserSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public IActionResult ExtensionSession(string session_token)
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

                #region Compare with present session token
                if (!Utils.IsValidSessionToken(session_token)) {
                    return Problem(400, "Invalid session token.");
                }
                if (session_token == sessionToken) {
                    return Problem(400, "Not allow delete session. Try logout.");
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;

                if (!__SessionSocialUserManagement.FindSessionForUse(sessionToken, EXPIRY_TIME, EXTENSION_TIME, out session, out error)) {
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
                    throw new Exception("Internal Server Error. FindSessionSocialForUse Failed.");
                }
                #endregion

                #region Delete session
                var user = session.User;
                SessionSocialUser delSession = null;
                if (!__SessionSocialUserManagement.FindSession(session_token, out delSession, out error)) {
                    LogInformation($"Delete session not found, session_token: { session_token.Substring(0, 15) }");
                    return Problem(404, "Delete session not found.");
                }
                if (!__SessionSocialUserManagement.RemoveSession(delSession, out error)) {
                    throw new Exception("Internal Server Error. DeleteSessionSocial Failed.");
                }
                #endregion

                LogInformation($"Delete session success, session_token: { session_token.Substring(0, 15) }");
                return Ok( new JObject(){
                    { "status", 200 },
                    { "message", "Success." },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
