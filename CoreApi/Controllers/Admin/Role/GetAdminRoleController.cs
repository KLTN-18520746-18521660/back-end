using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Admin.Role
{
    [ApiController]
    [Route("/api/admin/role/admin")]
    public class GetAdminRoleController : BaseController
    {
        public GetAdminRoleController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAdminRoles([FromServices] AdminUserManagement __AdminUserManagement,
                                                       [FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                       [FromHeader(Name = "session_token_admin")] string SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __AdminUserManagement,
                __SessionAdminUserManagement
            );
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

                #region Check Permission
                var Error = __AdminUserManagement.HaveReadPermission(Session.User.Rights, ADMIN_RIGHTS.ADMIN_USER);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(400, RESPONSE_MESSAGES.NOT_ALLOW_TO_DO, new string[]{ "see admin roles" });
                }
                #endregion

                #region Get all roles
                var (Roles, TotalSize) = await __AdminUserManagement.GetRoles();
                var Ret = new JArray();
                Roles.ForEach(e => Ret.Add(e.GetJsonObject()));
                #endregion

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "roles", Ret },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
