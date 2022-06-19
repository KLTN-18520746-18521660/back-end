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

namespace CoreApi.Controllers.Admin.Right
{
    [ApiController]
    [Route("/api/admin/right/admin")]
    public class NewAdminRightController : BaseController
    {
        public NewAdminRightController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [HttpPost("")]
        public async Task<IActionResult> NewAdminRight([FromServices] AdminUserManagement __AdminUserManagement,
                                                       [FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                       [FromBody] ParserAdminUserRight __ParserModel,
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
                var Error = __AdminUserManagement.HaveFullPermission(Session.User.Rights, ADMIN_RIGHTS.ADMIN_USER);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(400, RESPONSE_MESSAGES.NOT_ALLOW_TO_DO, new string[]{ "add new admin right" });
                }
                #endregion

                #region Parser Admin Right
                var Right = new AdminUserRight();
                if (!Right.Parse(__ParserModel, out var ErrorPaser)) {
                    AddLogParam("error_parser", ErrorPaser);
                    AddLogParam("model_data", __ParserModel);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_BODY);
                }
                #endregion

                #region Check right exist
                AdminUserRight FindRight = default;
                (FindRight, Error) = await __AdminUserManagement.GetRight(Right.RightName);
                if (Error == ErrorCodes.NO_ERROR) {
                    return Problem(400, RESPONSE_MESSAGES.ALREADY_EXIST, new string[]{ "Right" });
                }
                #endregion

                #region Add new right
                Error = await __AdminUserManagement.NewRight(Right, Session.UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"NewRight failed, ErrorCode: { Error }");
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
