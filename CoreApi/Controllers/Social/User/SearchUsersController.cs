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

namespace CoreApi.Controllers.Social.User
{
    [ApiController]
    [Route("/api/user/search")]
    public class SearchUsersController : BaseController
    {
        public SearchUsersController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> GetTrendingPosts([FromServices] SessionSocialUserManagement    __SessionSocialUserManagement,
                                                          [FromServices] SocialCategoryManagement       __SocialCategoryManagement,
                                                          [FromServices] SocialUserManagement           __SocialUserManagement,
                                                          [FromHeader(Name = "session_token")] string   SessionToken,
                                                          [FromQuery(Name = "start")] int               Start = 0,
                                                          [FromQuery(Name = "size")] int                Size = 20,
                                                          [FromQuery(Name = "search_term")] string      SearchTerm = default,
                                                          [FromQuery] Models.OrderModel                 Orders      = default)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionSocialUserManagement,
                __SocialCategoryManagement,
                __SocialUserManagement
            );
            #endregion
            try {
                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Validate params
                AddLogParam("size",        Size);
                AddLogParam("start",       Start);
                AddLogParam("orders",      Orders);
                AddLogParam("search_term", SearchTerm);
                if (Start < 0 || Size < 1) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                var AllowOrderParams                = __SocialUserManagement.GetAllowOrderFields(GetUserAction.SearchUsers);
                var (CombineOrders, ErrRetValidate) = ValidateOrderParams(Orders, AllowOrderParams);
                if (Start < 0 || Size < 1) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if (ErrRetValidate != default) {
                    return ErrRetValidate;
                }
                #endregion

                #region Get users
                var (Users, TotalSize, Error) = await __SocialUserManagement
                    .SearchUsers(
                        IsValidSession ? Session.UserId : default,
                        Start,
                        Size,
                        SearchTerm,
                        CombineOrders
                    );
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"SearchPosts failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                var Ret = new List<JObject>();
                Users.ForEach(e => {
                    var Obj = e.GetPublicJsonObject();
                    if (IsValidSession) {
                        if (Session.UserId == e.Id) {
                            Obj = e.GetJsonObject();
                        }
                        Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    }
                    Ret.Add(Obj);
                });

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "users",      Utils.ObjectToJsonToken(Ret) },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
