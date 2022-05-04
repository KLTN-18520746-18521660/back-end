using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Admin.Config
{
    [ApiController]
    [Route("/api/admin/config")]
    public class ReloadConfigController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public ReloadConfigController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "ReloadConfig";
            __IsAdminController = true;
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = string.Empty;
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
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

        /// <summary>
        /// Reload configs
        /// </summary>
        /// <param name="__AdminUserManagement"></param>
        /// <param name="__SessionAdminUserManagement"></param>
        /// <param name="session_token"></param>
        /// <returns><b>config value</b></returns>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header or cookie 'session_token_admin'.
        /// - User have full permission of 'config'.
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> message 'OK'.
        /// </response>
        /// 
        /// <response code="401">
        /// <b>Error case <i>(Server auto send response with will clear cookie 'session_token_admin')</i>, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
        /// <li>Session not found.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="403">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>User doesn't have permission to reload config.</li>
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
        [HttpPost("reload")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StatusCode200OKExamples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ReloadConfig([FromServices] AdminUserManagement __AdminUserManagement,
                                                      [FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                      [FromHeader(Name = "session_token_admin")] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __AdminUserManagement.SetTraceId(TraceId);
            __SessionAdminUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session token
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                    return Problem(401, "Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    LogDebug($"Invalid header authorization.");
                    return Problem(401, "Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionAdminUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionAdminUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogWarning($"Session not found, session_token: { session_token.Substring(0, 15) }");
                        return Problem(401, "Session not found.");
                    }
                    if (error == ErrorCodes.SESSION_HAS_EXPIRED) {
                        LogWarning($"Session has expired, session_token: { session_token.Substring(0, 15) }");
                        return Problem(401, "Session has expired.");
                    }
                    if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogWarning($"User has been locked, session_token: { session_token.Substring(0, 15) }");
                        return Problem(423, "You have been locked.");
                    }
                    throw new Exception($"FindSessionForUse Failed. ErrorCode: { error }");
                }
                #endregion

                #region Check Permission
                var user = session.User;
                if (__AdminUserManagement.HaveFullPermission(user.Rights, ADMIN_RIGHTS.CONFIG) == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    LogWarning($"User doesn't have permission for reload config, user_name: { user.UserName }");
                    return Problem(403, "User doesn't have permission for reload config.");
                }
                #endregion

                #region Reload all config
                error = await __BaseConfig.ReLoadConfig();
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"ReLoadConfig Failed. ErrorCode: { error }");
                }
                #endregion

                LogInformation($"Reload config successfully, user_name: { user.UserName }");
                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
