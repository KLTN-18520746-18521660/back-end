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

namespace CoreApi.Controllers.Social.Category
{
    [ApiController]
    [Route("/api/catrgory")]
    public class GetCategoriesByActionController : BaseController
    {
        public static string[] AllowActions = new string[]{
            "follow",
        };
        public GetCategoriesByActionController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpGet("action")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetCategoriesByAction([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                               [FromServices] SocialCategoryManagement     __SocialCategoryManagement,
                                                               [FromServices] SocialUserManagement         __SocialUserManagement,
                                                               [FromHeader(Name = "session_token")] string SessionToken,
                                                               [FromQuery(Name = "action")] string         Action,
                                                               [FromQuery(Name = "start")] int             Start       = 0,
                                                               [FromQuery(Name = "size")] int              Size        = 20,
                                                               [FromQuery(Name = "search_term")] string    SearchTerm  = default,
                                                               [FromQuery] Models.OrderModel               Orders      = default)
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
                #region Get session
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Validate params
                AddLogParam("start",        Start);
                AddLogParam("size",         Size);
                AddLogParam("search_term",  SearchTerm);
                AddLogParam("orders",       Orders);
                IActionResult ErrRetValidate    = default;
                (string, bool)[] CombineOrders  = default;
                var AllowOrderParams            = __SocialCategoryManagement.GetAllowOrderFields(GetCategoryAction.GetCategoriesByAction);
                (CombineOrders, ErrRetValidate) = ValidateOrderParams(Orders, AllowOrderParams);
                if (ErrRetValidate != default) {
                    return ErrRetValidate;
                }
                if (Start < 0 || Size < 1) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if (CombineOrders == default) {
                    throw new Exception($"ValidateOrderParams failed.");
                }
                if (Action == default || Action.Trim() == string.Empty || Action.Trim().Length > 50) {
                    AddLogParam("action", Action);
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                Action = Action.Trim().ToLower();
                // var Action = Action.Trim().ToLower();
                if (!AllowActions.Contains(Action)) {
                    AddLogParam("action", Action);
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }

                #region Get categories
                var (Categories, TotalSize, Error) = await __SocialCategoryManagement
                    .GetCategoriesByAction(
                        Session.UserId,
                        Action,
                        Start,
                        Size,
                        SearchTerm,
                        CombineOrders
                    );
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetCategoriesByAction failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                var Ret = new List<JObject>();
                foreach (var c in Categories) {
                    var Obj = c.GetPublicJsonObject();
                    Obj.Add("actions", Utils.ObjectToJsonToken(c.GetActionByUser(Session.UserId)));
                    Ret.Add(Obj);
                }

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "categories",      Utils.ObjectToJsonToken(Ret) },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
