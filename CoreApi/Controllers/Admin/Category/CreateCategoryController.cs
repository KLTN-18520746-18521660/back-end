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
    public class CreateCategoryController : BaseController
    {
        public CreateCategoryController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateCategory([FromServices] SessionAdminUserManagement    __SessionAdminUserManagement,
                                                       [FromServices] SocialCategoryManagement       __SocialCategoryManagement,
                                                       [FromServices] AdminUserManagement            __AdminUserManagement,
                                                       [FromBody] ParserSocialCategory               __ParserModel,
                                                       [FromHeader(Name = "session_token")] string   SessionToken)
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

                #region Parse Category
                var NewCategory = new SocialCategory();
                if (!NewCategory.Parse(__ParserModel, out var ErrorPaser)) {
                    AddLogParam("error_parser", ErrorPaser);
                    AddLogParam("model_data", __ParserModel);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_BODY);
                }
                #endregion

                #region Check Permission
                var IsHaveReadPermission = true;
                var Error = __AdminUserManagement.HaveFullPermission(Session.User.Rights, ADMIN_RIGHTS.CATEGORY);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(403, RESPONSE_MESSAGES.NOT_ALLOW_TO_DO, new string[]{ "create category" });
                    // IsHaveReadPermission = false;
                }
                AddLogParam("have_read_permission", IsHaveReadPermission);
                #endregion

                if (NewCategory.ParentId != default) {
                    SocialCategory Parent = default;
                    (Parent, Error) = await __SocialCategoryManagement.FindCategoryById((long) NewCategory.ParentId);
                    if (Error != ErrorCodes.NO_ERROR) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "parent category" });
                    }
                    NewCategory.Parent = Parent;
                }

                #region Add category
                Error = await __SocialCategoryManagement.AddNewCategory(NewCategory, Session.UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"AddNewCategory failed, ErrorCode: { Error }");
                }
                #endregion

                return Ok(201, RESPONSE_MESSAGES.OK);
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
