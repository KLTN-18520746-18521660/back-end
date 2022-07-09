using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using DatabaseAccess.Common.Status;
using Newtonsoft.Json;
using DatabaseAccess.Context.ParserModels;

namespace CoreApi.Controllers.Admin.Category
{
    [ApiController]
    [Route("/api/admin/category")]
    public class ModifyCategoryController : BaseController
    {
        private static string[] AllowStatus = new string[]{
            EntityStatus.StatusTypeToString(StatusType.Enabled),
            EntityStatus.StatusTypeToString(StatusType.Disabled),
            EntityStatus.StatusTypeToString(StatusType.Readonly),
        };
        public ModifyCategoryController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ModifyCategory([FromServices] SessionAdminUserManagement        __SessionAdminUserManagement,
                                                        [FromServices] SocialCategoryManagement          __SocialCategoryManagement,
                                                        [FromServices] AdminUserManagement               __AdminUserManagement,
                                                        [FromRoute(Name = "id")] long                    __Id,
                                                        [FromBody] SocialCategoryModifyModel             __ModelData,
                                                        [FromHeader(Name = "session_token")] string      SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionAdminUserManagement,
                __SocialCategoryManagement,
                __AdminUserManagement
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
                var IsHaveReadPermission = true;
                var Error = __AdminUserManagement.HaveFullPermission(Session.User.Rights, ADMIN_RIGHTS.TAG);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(403, RESPONSE_MESSAGES.NOT_ALLOW_TO_DO, new string[]{ "modify category" });
                    // IsHaveReadPermission = false;
                }
                AddLogParam("have_read_permission", IsHaveReadPermission);
                #endregion

                #region Find category
                SocialCategory FindCategory   = default;
                (FindCategory, Error)    = await __SocialCategoryManagement.FindCategoryById(__Id);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "category" });
                    }
                    throw new Exception($"FindCategoryById failed. ErrorCode: { Error }");
                }
                #endregion

                #region Validate status
                if (FindCategory.Status.Type == StatusType.Readonly && __ModelData.status != default && __ModelData.status != EntityStatus.StatusTypeToString(StatusType.Readonly)) {
                        return Problem(400, RESPONSE_MESSAGES.NOT_ALLOW_TO_DO, new string[]{ "change status" });
                }
                #endregion

                #region Modify category
                Error = await __SocialCategoryManagement.ModifyCategory(FindCategory.Id, __ModelData, Session.UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, RESPONSE_MESSAGES.NO_CHANGES_DETECTED);
                    }
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "parent category" });
                    }
                    throw new Exception($"ModifyCategory failed, ErrorCode: { Error }");
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
