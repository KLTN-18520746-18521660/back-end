using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Admin.User
{
    [ApiController]
    [Route("/admin/user")]
    public class CreateAdminUserController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minutes
        #endregion

        public CreateAdminUserController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "CreateAdminUser";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
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
        /// Create new admin user
        /// </summary>
        /// <param name="__AdminUserManagement"></param>
        /// <param name="__SessionAdminUserManagement"></param>
        /// <param name="session_token"></param>
        /// <param name="parser"></param>
        /// <returns><b>New admin user info</b></returns>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header 'session_token'.
        /// - User have full permission of 'admin_user'.
        /// 
        /// </remarks>
        ///
        /// <response code="201">
        /// <b>Success Case:</b> return new admin user ID.
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
        /// <li>User doesn't have permission to create admin user.</li>
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
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateAdminUserSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> CreateAdminUser([FromServices] AdminUserManagement __AdminUserManagement,
                                                         [FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                         [FromHeader] string session_token,
                                                         [FromBody] ParserAdminUser parser)
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
                if (session_token == null) {
                    LogDebug($"Missing header authorization.");
                    return Problem(403, "Missing header authorization.");
                }

                if (!Utils.IsValidSessionToken(session_token)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Parse Admin User
                AdminUser newUser = new AdminUser();
                string Error = "";
                if (!newUser.Parse(parser, out Error)) {
                    LogInformation(Error);
                    return Problem(400, "Bad request body.");
                }
                #endregion

                #region Find session for use
                SessionAdminUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionAdminUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { session_token.Substring(0, 15) }");
                        return Problem(400, "Session not found.");
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

                #region Check Permission
                var user = session.User;
                if (__AdminUserManagement.HaveFullPermission(user.Rights, ADMIN_RIGHTS.ADMIN_USER) == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    LogInformation($"User doesn't have permission to create admin user, user_name: { user.UserName }");
                    return Problem(403, "User doesn't have permission to create admin user.");
                }
                #endregion

                #region Check unique user_name, email
                AdminUser tmpUser = null;
                (tmpUser, error) = await __AdminUserManagement.FindUser(newUser.UserName, false);
                if (error == ErrorCodes.NO_ERROR) {
                    LogDebug($"UserName have been used, user_name: { newUser.UserName }");
                    return Problem(400, "UserName have been used.");
                }
                (tmpUser, error) = await __AdminUserManagement.FindUser(newUser.Email, true);
                if (error == ErrorCodes.NO_ERROR) {
                    LogDebug($"Email have been used, user_name: { newUser.Email }");
                    return Problem(400, "Email have been used.");
                }
                #endregion

                #region Add new admin user
                error = await __AdminUserManagement.AddNewUser(user.Id, newUser);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"AddNewAdminUser Failed. ErrorCode: { error }");
                }
                #endregion

                LogInformation($"Create new admin user success, user_name: { newUser.UserName }");
                return Ok(201, new JObject(){
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
