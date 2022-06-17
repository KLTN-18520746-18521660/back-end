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

namespace CoreApi.Controllers.Admin.Tag
{
    [ApiController]
    [Route("/api/admin/tag")]
    public class ModifyTagController : BaseController
    {
        private static string[] AllowStatus = new string[]{
            EntityStatus.StatusTypeToString(StatusType.Enabled),
            EntityStatus.StatusTypeToString(StatusType.Disabled),
            EntityStatus.StatusTypeToString(StatusType.Readonly),
        };
        public ModifyTagController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ModifyTag([FromServices] SessionAdminUserManagement     __SessionAdminUserManagement,
                                                   [FromServices] SocialTagManagement            __SocialTagManagement,
                                                   [FromServices] AdminUserManagement            __AdminUserManagement,
                                                   [FromRoute(Name = "id")] long                 __Id,
                                                   [FromBody] SocialTagModifyModel               __ModelData,
                                                   [FromHeader(Name = "session_token")] string   SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionAdminUserManagement,
                __SocialTagManagement,
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
                    IsHaveReadPermission = false;
                }
                AddLogParam("have_read_permission", IsHaveReadPermission);
                #endregion

                #region Find tag
                SocialTag FindTag   = default;
                (FindTag, Error)    = await __SocialTagManagement.FindTagById(__Id);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "tag" });
                    }
                    throw new Exception($"FindTagById failed. ErrorCode: { Error }");
                }
                #endregion

                #region Validate status
                if (FindTag.Status.Type == StatusType.Readonly && __ModelData.status != default && __ModelData.status != EntityStatus.StatusTypeToString(StatusType.Readonly)) {
                        return Problem(400, RESPONSE_MESSAGES.NOT_ALLOW_TO_DO, new string[]{ "change status" });
                }
                #endregion

                #region Modify tag
                Error = await __SocialTagManagement.ModifyTag(FindTag.Id, __ModelData, Session.UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, RESPONSE_MESSAGES.NO_CHANGES_DETECTED);
                    }
                    throw new Exception($"ModifyTag failed, ErrorCode: { Error }");
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
