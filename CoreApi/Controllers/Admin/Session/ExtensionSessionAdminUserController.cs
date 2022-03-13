using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Common;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using DatabaseAccess.Common.Status;

namespace CoreApi.Controllers.Admin.Session
{
    [ApiController]
    [Route("/admin/session")]
    public class ExtensionSessionAdminUserController : BaseController
    {
        #region Services
        private BaseConfig __BaseConfig;
        private SessionAdminUserManagement __SessionAdminUserManagement;
        #endregion

        #region Config Value
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minutes
        #endregion

        public ExtensionSessionAdminUserController(
            BaseConfig _BaseConfig,
            SessionAdminUserManagement _SessionAdminUserManagement
        ) : base() {
            __BaseConfig = _BaseConfig;
            __SessionAdminUserManagement = _SessionAdminUserManagement;
            __ControllerName = "ExtensionSessionAdminUser";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                EXTENSION_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, "extension_time", out Error);
                EXPIRY_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, "expiry_time", out Error);
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
        /// Extension admin session
        /// </summary>
        /// <returns><b>Extension admin session</b></returns>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header 'session_token'.
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
        [HttpPost("extension")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExtensionSessionAdminUserSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public IActionResult ExtensionSession()
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
                SessionAdminUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;

                if (!__SessionAdminUserManagement.FindSessionForUse(sessionToken, EXPIRY_TIME, EXTENSION_TIME, out session, out error)) {
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
                    throw new Exception("Internal Server Error. FindSessionForUse Failed.");
                }
                #endregion

                LogDebug($"Session extension success, session_token: { sessionToken.Substring(0, 15) }");
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
