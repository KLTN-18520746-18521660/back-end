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

namespace CoreApi.Controllers.Social.Post
{
    [ApiController]
    [Route("/api/post")]
    public class GetPostsByActionController : BaseController
    {
        public static string[] AllowActions = new string[]{
            "saved",
            "comment",
            "like",
            "dislike",
            "visited",
            "report",
            "follow",
        };
        public GetPostsByActionController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpGet("action")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPostsByAction([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                          [FromServices] SocialCategoryManagement     __SocialCategoryManagement,
                                                          [FromServices] SocialUserManagement         __SocialUserManagement,
                                                          [FromServices] SocialPostManagement         __SocialPostManagement,
                                                          [FromServices] SocialTagManagement          __SocialTagManagement,
                                                          [FromHeader(Name = "session_token")] string SessionToken,
                                                          [FromQuery(Name = "action")] string         Action,
                                                          [FromQuery(Name = "start")] int             Start       = 0,
                                                          [FromQuery(Name = "size")] int              Size        = 20,
                                                          [FromQuery(Name = "search_term")] string    SearchTerm  = default,
                                                          [FromQuery(Name = "tags")] string           Tags        = default,
                                                          [FromQuery(Name = "categories")] string     Categories  = default,
                                                          [FromQuery] Models.OrderModel               Orders      = default)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionSocialUserManagement,
                __SocialCategoryManagement,
                __SocialUserManagement,
                __SocialPostManagement,
                __SocialTagManagement
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
                AddLogParam("start", Start);
                AddLogParam("size", Size);
                AddLogParam("search_term", SearchTerm);
                AddLogParam("categories", Categories);
                AddLogParam("tags", Tags);
                AddLogParam("orders", Orders);
                IActionResult ErrRetValidate    = default;
                (string, bool)[] CombineOrders  = default;
                var AllowOrderParams            = __SocialPostManagement.GetAllowOrderFields(GetPostAction.GetPostsByAction);
                var TagsArr                     = Tags == default ? default : Tags.Split(',');
                var CategoriesArr               = Categories == default ? default : Categories.Split(',');
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
                if (Categories != default && !await __SocialCategoryManagement.IsExistingCategories(CategoriesArr)) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "categories" });
                }
                if (Tags != default && !await __SocialTagManagement.IsExistsTags(TagsArr)) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "tags" });
                }
                #endregion

                Action = Action.Trim().ToLower();
                // var Action = Action.Trim().ToLower();
                if (!AllowActions.Contains(Action)) {
                    AddLogParam("action", Action);
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }

                #region Get posts
                var (Posts, TotalSize, Error) = await __SocialPostManagement
                    .GetPostsByAction(
                        Session.UserId,
                        Action,
                        Start,
                        Size,
                        SearchTerm,
                        CombineOrders,
                        TagsArr,
                        CategoriesArr
                    );
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetPostsByAction failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                var Ret = new List<JObject>();
                foreach (var p in Posts) {
                    var Obj = p.GetPublicShortJsonObject(Session.UserId);
                    if (p.StatusStr != EntityStatus.StatusTypeToString(StatusType.Approved)) {
                        TotalSize--;
                        continue;
                        // if (Action.ToLower() != "saved") {
                        //     continue;
                        // }
                        // Obj = Utils.MakeValueJSonEmpty(Obj, new string[]{
                        //     "status",
                        // });
                    } else {
                        Obj.Add("actions", Utils.ObjectToJsonToken(p.GetActionByUser(Session.UserId)));
                    }
                    Ret.Add(Obj);
                }

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "posts",      Utils.ObjectToJsonToken(Ret) },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
