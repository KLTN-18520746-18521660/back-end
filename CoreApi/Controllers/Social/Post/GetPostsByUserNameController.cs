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
    public class GetPostsByUserNameController : BaseController
    {
        public GetPostsByUserNameController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        /// <summary>
        /// Get all post attach to user
        /// </summary>
        /// <returns><b>Social user of session_token</b></returns>
        /// <param name="__SessionSocialUserManagement"></param>
        /// <param name="__SocialCategoryManagement"></param>
        /// <param name="__SocialUserManagement"></param>
        /// <param name="__SocialPostManagement"></param>
        /// <param name="__SocialTagManagement"></param>
        /// <param name="__UserName"></param>
        /// <param name="SessionToken"></param>
        /// <param name="Start"></param>
        /// <param name="Size"></param>
        /// <param name="SearchTerm"></param>
        /// <param name="Tags"></param>
        /// <param name="Categories"></param>
        /// <param name="Status"></param>
        /// <param name="Orders"></param>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Header 'session_token' is optional.
        /// - If have session_token --> compare user is owner ?
        ///     - Is owner --> return full info of post (include post is pendding, reject, private)
        ///     - Else --> return info of public post (just post approve)
        /// - <i>Not allow search post of user have delete status.</i>
        /// - Must have query params for paging 'first', 'size'
        /// - Support query params 'status' (approve | pendding | reject | private) for filter
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> Social session of user.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Invalid slug.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpGet("user/{user_name}")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        // [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        // [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPostsByUserName([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                            [FromServices] SocialCategoryManagement     __SocialCategoryManagement,
                                                            [FromServices] SocialUserManagement         __SocialUserManagement,
                                                            [FromServices] SocialPostManagement         __SocialPostManagement,
                                                            [FromServices] SocialTagManagement          __SocialTagManagement,
                                                            [FromRoute(Name = "user_name")] string      __UserName,
                                                            [FromHeader(Name = "session_token")] string SessionToken,
                                                            [FromQuery(Name = "start")] int             Start       = 0,
                                                            [FromQuery(Name = "size")] int              Size        = 20,
                                                            [FromQuery(Name = "search_term")] string    SearchTerm  = default,
                                                            [FromQuery(Name = "tags")] string           Tags        = default,
                                                            [FromQuery(Name = "categories")] string     Categories  = default,
                                                            [FromQuery(Name = "status")] string         Status      = default,
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
                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
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
                string[] StatusArr              = default;
                var AllowOrderParams            = __SocialPostManagement.GetAllowOrderFields(GetPostAction.GetPostsAttachedToUser);
                var TagsArr                     = Tags == default ? default : Tags.Split(',');
                var CategoriesArr               = Categories == default ? default : Categories.Split(',');
                (CombineOrders, ErrRetValidate) = ValidateOrderParams(Orders, AllowOrderParams);
                if (ErrRetValidate != default) {
                    return ErrRetValidate;
                }
                if (CombineOrders == default) {
                    throw new Exception($"ValidateOrderParams failed.");
                }
                (StatusArr, ErrRetValidate) = ValidateStatusParams(Status, new StatusType[] { StatusType.Deleted });
                if (ErrRetValidate != default) {
                    return ErrRetValidate;
                }
                if (StatusArr == default) {
                    throw new Exception($"ValidateStatusParams failed.");
                }
                if (__UserName == default || __UserName.Trim() == string.Empty || __UserName.Length > 50) {
                    AddLogParam("user_name", __UserName);
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if (Categories != default && !await __SocialCategoryManagement.IsExistingCategories(CategoriesArr)) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "categories" });
                }
                if (Tags != default && !await __SocialTagManagement.IsExistsTags(TagsArr)) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "tags" });
                }
                #endregion

                #region Get posts
                var (PostUser, Error) = await __SocialUserManagement.FindUserIgnoreStatus(__UserName, false);
                if (Error != ErrorCodes.NO_ERROR) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "user" });
                }
                List<SocialPost> Posts  = default;
                int TotalSize           = default;
                (Posts, TotalSize, Error) = await __SocialPostManagement
                    .GetPostsAttachedToUser(
                        PostUser.Id,
                        IsValidSession ? Session.UserId == PostUser.Id : false,
                        Start,
                        Size,
                        SearchTerm,
                        StatusArr,
                        CombineOrders,
                        TagsArr,
                        CategoriesArr
                    );
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetPostsAttachedToUser failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                var Ret = new List<JObject>();
                Posts.ForEach(e => {
                    var Obj = e.GetPublicShortJsonObject(IsValidSession ? Session.UserId : default);
                    if (IsValidSession) {
                        Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    }
                    Ret.Add(Obj);
                });

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
