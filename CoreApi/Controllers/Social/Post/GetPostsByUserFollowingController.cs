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
    public class GetPostsByUserFollowingController : BaseController
    {
        public GetPostsByUserFollowingController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpGet("following")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPostsByUserFollowing([FromServices] SessionSocialUserManagement     __SessionSocialUserManagement,
                                                                 [FromServices] SocialCategoryManagement        __SocialCategoryManagement,
                                                                 [FromServices] SocialUserManagement            __SocialUserManagement,
                                                                 [FromServices] SocialPostManagement            __SocialPostManagement,
                                                                 [FromServices] SocialTagManagement             __SocialTagManagement,
                                                                 [FromHeader(Name = "session_token")] string    SessionToken,
                                                                 [FromQuery(Name = "start")] int                Start       = 0,
                                                                 [FromQuery(Name = "size")] int                 Size        = 20,
                                                                 [FromQuery(Name = "search_term")] string       SearchTerm  = default,
                                                                 [FromQuery(Name = "tags")] string              Tags        = default,
                                                                 [FromQuery(Name = "categories")] string        Categories  = default,
                                                                 [FromQuery] Models.OrderModel                  Orders      = default)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SocialCategoryManagement,
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
                var AllowOrderParams       = __SocialPostManagement.GetAllowOrderFields(GetPostAction.GetPostsByUserFollowing);
                var TagsArr                = Tags == default ? default : Tags.Split(',');
                var CategoriesArr          = Categories == default ? default : Categories.Split(',');
                var (CombineOrders, ErrRetValidate) = ValidateOrderParams(Orders, AllowOrderParams);
                if (ErrRetValidate != default) {
                    return ErrRetValidate;
                }
                if (CombineOrders == default) {
                    throw new Exception($"ValidateOrderParams failed.");
                }
                if (Categories != default && !await __SocialCategoryManagement.IsExistingCategories(CategoriesArr)) {
                    return Problem(400, "Invalid categories not exists.");
                }
                if (Tags != default && !await __SocialTagManagement.IsExistsTags(TagsArr)) {
                    return Problem(400, "Invalid tags not exists.");
                }
                #endregion

                #region Get posts
                var (Posts, TotalSize, Error) = await __SocialPostManagement
                    .GetPostsByUserFollowing(
                        Session.UserId,
                        Start,
                        Size,
                        CombineOrders,
                        TagsArr,
                        CategoriesArr
                    );
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetPostsByUserFollowing failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, $"Invalid request params start: { Start }. Total size is { TotalSize }");
                }
                #endregion

                var Ret = new List<JObject>();
                Posts.ForEach(e => {
                    var Obj = e.GetPublicShortJsonObject();
                    Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    Ret.Add(Obj);
                });

                return Ok(200, "OK", new JObject(){
                    { "posts",      Utils.ObjectToJsonToken(Ret) },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
