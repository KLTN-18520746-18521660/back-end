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

namespace CoreApi.Controllers.Admin.Right
{
    [ApiController]
    [Route("/api/admin/right/admin")]
    public class GetAdminRightController  : BaseController
    {
        public GetAdminRightController (BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAdminRights([FromServices] AdminUserManagement __AdminUserManagement,
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
                    return Problem(400, RESPONSE_MESSAGES.NOT_ALLOW_TO_DO, new string[]{ "see admin rights" });
                }
                #endregion

                #region Get all rights
                var (Rights, TotalSize) = await __AdminUserManagement.GetRights();
                var Ret = new JArray();
                Rights.ForEach(e => Ret.Add(e.GetJsonObject()));
                #endregion

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "rights", Ret },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
