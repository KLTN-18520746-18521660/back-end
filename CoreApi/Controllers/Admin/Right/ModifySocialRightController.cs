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
using DatabaseAccess.Context.ParserModels;
using CoreApi.Models.ModifyModels;
using DatabaseAccess.Common.Status;

namespace CoreApi.Controllers.Admin.Right
{
    [ApiController]
    [Route("/api/admin/right/social")]
    public class ModifySocialRightController : BaseController
    {
        public ModifySocialRightController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ModifySocialRight([FromServices] AdminUserManagement __AdminUserManagement,
                                                           [FromServices] SocialUserManagement __SocialUserManagement,
                                                           [FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                           [FromBody] SocialUserRightModifyModel __ModelData,
                                                           [FromRoute(Name = "id")] int __Id,
                                                           [FromHeader(Name = "session_token_admin")] string SessionToken)
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

                #region Check Permission
                var Error = __AdminUserManagement.HaveFullPermission(Session.User.Rights, ADMIN_RIGHTS.SOCIAL_USER);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(400, RESPONSE_MESSAGES.NOT_ALLOW_TO_DO, new string[]{ "modify social right" });
                }
                #endregion

                #region Get right info
                SocialUserRight FindRight = default;
                (FindRight, Error) = await __SocialUserManagement.GetRightById(__Id);
                if (Error != ErrorCodes.NO_ERROR || FindRight.Status.Type == StatusType.Disabled) {
                    if (Error == ErrorCodes.NOT_FOUND || FindRight.Status.Type == StatusType.Disabled) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "right" });
                    }
                    throw new Exception($"GetRightById failed, ErrorCode: { Error }");
                }
                #endregion

                #region Modify right
                Error = await __SocialUserManagement.ModifyRight(FindRight.Id, __ModelData,  Session.UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, RESPONSE_MESSAGES.NO_CHANGES_DETECTED);
                    }
                    throw new Exception($"ModifyRight failed, ErrorCode: { Error }");
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
