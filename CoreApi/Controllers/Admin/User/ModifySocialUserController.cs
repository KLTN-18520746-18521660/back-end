using Common;
using CoreApi.Common.Base;
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
using System.Linq;
using System.Threading.Tasks;
using CoreApi.Models.ModifyModels;
using DatabaseAccess.Common.Status;

namespace CoreApi.Controllers.Admin.User
{
    [ApiController]
    [Route("/api/admin/user")]
    public class ModifySocialUserController : BaseController
    {
        private static string[] AllowStatus = new string[]{
            EntityStatus.StatusTypeToString(StatusType.Activated),
            EntityStatus.StatusTypeToString(StatusType.Blocked),
        };
        public ModifySocialUserController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [HttpPut("social/{id}")]
        public async Task<IActionResult> ModifySocialUser([FromServices] AdminUserManagement                 __AdminUserManagement,
                                                          [FromServices] SocialUserManagement                __SocialUserManagement,
                                                          [FromServices] SessionAdminUserManagement          __SessionAdminUserManagement,
                                                          [FromRoute(Name = "id")] Guid                      __Id,
                                                          [FromBody] SocialUserModifyModelByAdmin            __ModelData,
                                                          [FromHeader(Name = "session_token_admin")] string  SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __AdminUserManagement,
                __SocialUserManagement,
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

                #region Validate params
                if (__ModelData.status != default && !AllowStatus.Contains(__ModelData.status)) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                #region Check Permission
                var Error = __AdminUserManagement.HaveFullPermission(Session.User.Rights, ADMIN_RIGHTS.SOCIAL_USER);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(403, RESPONSE_MESSAGES.USER_DOES_NOT_HAVE_PERMISSION, new string[]{ "modify admin user" });
                }
                #endregion

                #region Find user
                SocialUser FindUser      = default;
                (FindUser, Error)       = await __SocialUserManagement.FindUserById(__Id);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND || FindUser.Status.Type == StatusType.Deleted) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "user" });
                    }
                    throw new Exception($"FindUserById failed. ErrorCode: { Error }");
                }
                #endregion

                #region Modify user
                Error = await __SocialUserManagement.ModifyUser(__Id, __ModelData, Session.UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, RESPONSE_MESSAGES.NO_CHANGES_DETECTED);
                    }
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "'role'" });
                    }
                    throw new Exception($"ModifyUser failed. ErrorCode: { Error }");
                }
                if (FindUser.Status.Type == StatusType.Deleted) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "user" });
                }
                #endregion

                return Ok(200, RESPONSE_MESSAGES.OK);
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
