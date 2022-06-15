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
using System.Collections.Generic;

namespace CoreApi.Controllers.Admin.User
{
    [ApiController]
    [Route("/api/admin/user_social")]
    public class GetSocialUsersController : BaseController
    {
        public GetSocialUsersController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }
        [HttpGet("")]
        public async Task<IActionResult> GetSocialUsers([FromServices] SocialUserManagement __SocialUserManagement,
                                                        [FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                        [FromHeader(Name = "session_token_admin")] string SessionToken,
                                                        [FromQuery(Name = "start")] int Start = 0,
                                                        [FromQuery(Name = "size")] int Size = 20,
                                                        [FromQuery(Name = "search_term")] string SearchTerm = default)
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

                #region Validate params
                AddLogParam("start",       Start);
                AddLogParam("size",        Size);
                AddLogParam("search_term", SearchTerm);
                if (Start < 0 || Size < 1) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                #region Get Social Users
                var (Users, TotalSize) = await __SocialUserManagement.GetUsers(Start, Size, SearchTerm);
                var RawRet             = new List<JObject>();
                Users.ForEach(e => {
                    var obj = e.GetPublicJsonObject();
                    obj.Add("id", e.Id);
                    RawRet.Add(obj);
                });
                var Ret = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(RawRet));
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "users", Ret },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
