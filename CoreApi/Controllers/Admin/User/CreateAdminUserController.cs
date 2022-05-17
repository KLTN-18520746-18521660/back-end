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
    [Route("/api/admin/user")]
    public class CreateAdminUserController : BaseController
    {
        public CreateAdminUserController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "CreateAdminUser";
            IsAdminController = true;
        }

        /// <summary>
        /// Create new admin user
        /// </summary>
        /// <param name="__AdminUserManagement"></param>
        /// <param name="__SessionAdminUserManagement"></param>
        /// <param name="__ParserModel"></param>
        /// <param name="SessionToken"></param>
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
        public async Task<IActionResult> CreateAdminUser([FromServices] AdminUserManagement                 __AdminUserManagement,
                                                         [FromServices] SessionAdminUserManagement          __SessionAdminUserManagement,
                                                         [FromBody] ParserAdminUser                         __ParserModel,
                                                         [FromHeader(Name = "session_token_admin")] string  SessionToken)
        {
            #region Set TraceId for services
            __AdminUserManagement.SetTraceId(TraceId);
            __SessionAdminUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionAdminUserManagement, SessionToken);
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session             = __Session as SessionAdminUser;
                #endregion

                #region Parse Admin User
                AdminUser NewUser = new AdminUser();
                if (!NewUser.Parse(__ParserModel, out var ErrorPaser)) {
                    AddLogParam("error_parser", ErrorPaser);
                    AddLogParam("model_data", __ParserModel);
                    return Problem(400, "Bad request body.");
                }
                #endregion

                #region Check Permission
                var Error = __AdminUserManagement.HaveFullPermission(Session.User.Rights, ADMIN_RIGHTS.ADMIN_USER);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(403, "User doesn't have permission to create admin user.");
                }
                #endregion

                #region Check unique user_name, email
                bool UsernameExisted = false, EmailExisted = false;
                (UsernameExisted, EmailExisted, Error) = await __AdminUserManagement.IsUserExsiting(NewUser.UserName, NewUser.Email);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"IsUserExsiting Failed. ErrorCode: { Error }");
                } else if (UsernameExisted) {
                    return Problem(400, "UserName have been used.");
                } else if (EmailExisted) {
                    return Problem(400, "Email have been used.");
                }
                #endregion

                #region Check password policy
                var ErroMsg = __AdminUserManagement.ValidatePasswordWithPolicy(__ParserModel.password);
                if (ErroMsg != string.Empty) {
                    return Problem(400, ErroMsg);
                }
                #endregion

                #region Add new admin user
                Error = await __AdminUserManagement.AddNewUser(Session.UserId, NewUser);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"AddNewAdminUser Failed. ErrorCode: { Error }");
                }
                #endregion

                return Ok(201, "OK", new JObject(){
                    { "user_id", NewUser.Id },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
