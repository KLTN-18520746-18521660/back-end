using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Admin.User
{
    [ApiController]
    [Route("/api/admin/user_social")]
    public class GetSocialUserByIdController : BaseController
    {
        public GetSocialUserByIdController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSocialUserById([FromServices] SocialUserManagement __SocialUserManagement,
                                                           [FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                           [FromRoute(Name = "id")] Guid __Id,
                                                           [FromHeader(Name = "session_token_admin")] string SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SocialUserManagement, __SessionAdminUserManagement);
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
                var Error = __SocialUserManagement.HaveReadPermission(Session.User.Rights, ADMIN_RIGHTS.SOCIAL_USER);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(403, RESPONSE_MESSAGES.USER_DOES_NOT_HAVE_PERMISSION, new string[]{ "get social user" });
                }
                #endregion

                #region Get Social user info by id
                SocialUser RetUser = default;
                (RetUser, Error) = await __SocialUserManagement.FindUserById(__Id);
                AddLogParam("get_user_id", __Id);
                if (Error != ErrorCodes.NO_ERROR) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "user" });
                }
                if (RetUser.Status.Type == StatusType.Deleted) {
                    AddLogParam("user_status", RetUser.StatusStr);
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "user" });
                }
                #endregion

                var Ret = RetUser.GetPublicJsonObject();
                Ret.Add("id", RetUser.Id);
                Ret.Add("roles", Utils.ObjectToJsonToken(RetUser.Roles));
                Ret.Add("rights", Utils.ObjectToJsonToken(RetUser.Rights));
                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "user", Ret },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
