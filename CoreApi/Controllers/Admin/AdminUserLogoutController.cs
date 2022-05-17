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

namespace CoreApi.Controllers.Admin
{
    [ApiController]
    [Route("/api/admin/logout")]
    public class AdminUserLogoutController : BaseController
    {
        public AdminUserLogoutController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "AdminUserLogout";
            IsAdminController = true;
        }

        /// <summary>
        /// Admin user logout
        /// </summary>
        /// <returns><b>Return message ok</b></returns>
        /// <param name="__SessionAdminUserManagement"></param>
        /// <param name="SessionToken"></param>
        ///
        /// <remarks>
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> return message <q>Success.</q>.
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
        /// <b>Error case <i>(Server auto send response with will clear cookie 'session_token_admin')</i>, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
        /// <li>Session not found.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminUserLogoutSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> AdminUserLogout([FromServices] SessionAdminUserManagement          __SessionAdminUserManagement,
                                                         [FromHeader(Name = "session_token_admin")] string  SessionToken)
        {
            #region Set TraceId for services
            __SessionAdminUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get config values
                var ExpiryTime      = GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                #endregion

                #region Validate session token
                AddLogParam(SessionTokenHeaderKey, SessionToken);
                SessionToken = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                if (SessionToken == default) {
                    return Problem(401, "Missing header authorization.", default, LOG_LEVEL.DEBUG);
                }

                if (!CommonValidate.IsValidSessionToken(SessionToken)) {
                    return Problem(401, "Invalid header authorization.", default, LOG_LEVEL.DEBUG);
                }
                #endregion

                #region Find session token
                var (Session, Error) = await __SessionAdminUserManagement.FindSession(SessionToken);

                if (Error != ErrorCodes.NO_ERROR) {
                    return Problem(401, "Session not found.", default, LOG_LEVEL.DEBUG);
                }
                #endregion

                #region Remove session and clear expried session
                Error = await __SessionAdminUserManagement.RemoveSession(Session.SessionToken);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"RemoveSessionAdminUser failed. ErrorCode: { Error }");
                }
                Error = await __SessionAdminUserManagement.ClearExpiredSession(Session.User.GetExpiredSessions(ExpiryTime));
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"ClearExpiredSessionAdminUser failed. ErrorCode: { Error }");
                }
                #endregion

                #region Set cookie header to remove session_token_admin
                Response.Cookies.Append(SessionTokenHeaderKey, string.Empty, GetCookieOptionsForDelete());
                #endregion

                return Ok(200, "OK");
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
