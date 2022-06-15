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

namespace CoreApi.Controllers.Admin.Tag
{
    [ApiController]
    [Route("/api/admin/tag")]
    public class GetTagsController : BaseController
    {
        public GetTagsController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> GetCategories([FromServices] SessionAdminUserManagement     __SessionAdminUserManagement,
                                                       [FromServices] AdminUserManagement            __AdminUserManagement,
                                                       [FromServices] SocialTagManagement            __SocialTagManagement,
                                                       [FromHeader(Name = "session_token")] string   SessionToken,
                                                       [FromQuery(Name = "start")] int               Start       = 0,
                                                       [FromQuery(Name = "size")] int                Size        = 20,
                                                       [FromQuery(Name = "search_term")] string      SearchTerm  = default)
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

                #region Check Permission
                var IsHaveReadPermission = true;
                var Error = __AdminUserManagement.HaveReadPermission(Session.User.Rights, ADMIN_RIGHTS.TAG);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    IsHaveReadPermission = false;
                }
                AddLogParam("have_read_permission", IsHaveReadPermission);
                #endregion

                #region Validate params
                AddLogParam("size",         Size);
                AddLogParam("start",        Start);
                AddLogParam("search_term",  SearchTerm);
                if (Start < 0 || Size < 1) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                #region Get tags
                var (Tags, TotalSize) = await __SocialTagManagement
                    .SearchTags(
                        Start,
                        Size,
                        SearchTerm,
                        default,
                        true
                    );
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                var Ret = new List<JObject>();
                Tags.ForEach(e => {
                    var Obj = e.GetJsonObject();
                    Ret.Add(Obj);
                });

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "tags",           Utils.ObjectToJsonToken(Ret) },
                    { "total_size",     TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
