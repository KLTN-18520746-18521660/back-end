using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;


using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using System.Net.Http;
using Swashbuckle;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.SwaggerUI;
// using Swashbuckle.Examples;
// using System.Net;
// using Swashbuckle.Swagger.Annotations;

namespace CoreApi.Controllers.Admin.User
{
    [ApiController]
    [Route("/admin/user")]
    public class CreateAdminUserController : BaseController
    {
        #region Services
        private BaseConfig __BaseConfig;
        private AdminUserManagement __AdminUserManagement;
        private SessionAdminUserManagement __SessionAdminUserManagement;
        #endregion

        #region Config Value
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minutes
        #endregion

        public CreateAdminUserController(
            BaseConfig _BaseConfig,
            AdminUserManagement _AdminUserManagement,
            SessionAdminUserManagement _SessionAdminUserManagement
        ) : base() {
            __BaseConfig = _BaseConfig;
            __AdminUserManagement = _AdminUserManagement;
            __SessionAdminUserManagement = _SessionAdminUserManagement;
            __ControllerName = "CreateAdminUser";
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
                StringBuilder msg = new StringBuilder(e.Message);
                if (Error != e.Message && Error != "") {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value fail, message: { msg }");
            }
        }

        /// <summary>
        /// Create new admin user
        /// </summary>
        /// <param name="parser"></param>
        /// <returns>New admin user info</returns>
        ///
        /// <remarks>
        /// Using endpoint:
        /// 
        ///     Need session_token and full right of "admin_user".
        /// 
        /// </remarks>
        ///
        /// <response code="201">
        ///     Return new admin user info.
        /// </response>
        /// <response code="400">
        ///     Bad request body.
        ///     Field 'user_name' or 'email' has been used.
        /// </response>
        /// <response code="401">
        ///     Session has expired.
        /// </response>
        /// <response code="403">
        ///     Missing header session_token.
        ///     Header session_token is invalid.
        ///     User doesn't have permission to create admin user.
        /// </response>
        /// <response code="500">
        ///     Internal Server Error.
        /// </response>
        [HttpPost("")]
        // [ApiExplorerSettings(GroupName = "admin")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateAdminUserSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public IActionResult CreateAdminUser(ParserAdminUser parser)
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

                if (!CoreApi.Common.Utils.IsValidSessionToken(sessionToken)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Parse Admin User
                AdminUser newUser = new AdminUser();
                string Error = "";
                if (!newUser.Parse(parser, out Error)) {
                    LogInformation(Error);
                    return Problem(400, "Bad body data.");
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
                    throw new Exception("Internal Server Error. FindSessionForUse Failed.");
                }
                #endregion

                #region Check Permission
                var user = session.User;
                if (!__AdminUserManagement.HaveFullPermission(user, ADMIN_RIGHTS.ADMIN_USER)) {
                    LogInformation($"User doesn't have permission to create admin user, user_name: { user.UserName }");
                    return Problem(403, "User doesn't have permission to create admin user.");
                }
                #endregion

                #region Check unique user_name, email
                AdminUser tmpUser = null;
                if (__AdminUserManagement.FindUser(newUser.UserName, false, out tmpUser, out error)) {
                    LogDebug($"UserName have been used, user_name: { user.UserName }");
                    return Problem(400, "UserName have been used.");
                }
                if (__AdminUserManagement.FindUser(newUser.Email, true, out tmpUser, out error)) {
                    LogDebug($"Email have been used, user_name: { user.Email }");
                    return Problem(400, "Email have been used.");
                }
                #endregion

                #region Add New Admin User
                if (!__AdminUserManagement.AddNewUser(user, newUser, out error)) {
                    throw new Exception("Internal Server Error. AddNewAdminUser Failed.");
                }
                #endregion

                LogInformation($"Create new admin user success, user_name: { newUser.UserName }");
                return Ok(201, new JObject(){
                    { "status", 201 },
                    { "message", "success" },
                    { "user_id", newUser.Id },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.Message }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
    class CreateAdminUserSuccessExample {
        [DefaultValue(201)]
        public int status { get; set; }
        [DefaultValue("success")]
        public string message { get; set; }
        [DefaultValue("94571498-b724-47c9-b046-1e932d5ec192")]
        public Guid user_id { get; set; }
    }
}
