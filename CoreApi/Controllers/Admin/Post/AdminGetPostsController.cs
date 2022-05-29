using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CoreApi.Controllers.Admin.Post
{
    [ApiController]
    [Route("/api/admin/posts")]
    public class AdminGetPostsController : BaseController
    {
        public AdminGetPostsController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetPostByIdSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPostById([FromServices] SessionAdminUserManagement          __SessionAdminUserManagement,
                                                     [FromServices] SocialCategoryManagement            __SocialCategoryManagement,
                                                     [FromServices] SocialPostManagement                __SocialPostManagement,
                                                     [FromServices] AdminUserManagement                 __AdminUserManagement,
                                                     [FromServices] SocialTagManagement                 __SocialTagManagement,
                                                     [FromHeader(Name = "session_token_admin")] string  SessionToken,
                                                     [FromQuery(Name = "start")] int                    Start = 0,
                                                     [FromQuery(Name = "size")] int                     Size = 20,
                                                     [FromQuery(Name = "search_term")] string           SearchTerm  = default,
                                                     [FromQuery(Name = "tags")] string                  Tags        = default,
                                                     [FromQuery(Name = "categories")] string            Categories  = default,
                                                     [FromQuery(Name = "status")] string                Status      = default,
                                                     [FromQuery(Name = "owner")] string                 Owner       = default,
                                                     [FromQuery] Models.OrderModel                      Orders      = default)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionAdminUserManagement,
                __SocialCategoryManagement,
                __SocialPostManagement,
                __AdminUserManagement,
                __SocialTagManagement
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
                AddLogParam("start", Start);
                AddLogParam("size", Size);
                AddLogParam("search_term", SearchTerm);
                AddLogParam("categories", Categories);
                AddLogParam("tags", Tags);
                AddLogParam("status", Status);
                AddLogParam("orders", Orders);
                AddLogParam("owner", Owner);
                IActionResult ErrRetValidate    = default;
                (string, bool)[] CombineOrders  = default;
                string[] StatusArr              = default;
                var AllowOrderParams            = __SocialPostManagement.GetAllowOrderFields(GetPostAction.GetPostsByAdminUser);
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
                if (Categories != default && !await __SocialCategoryManagement.IsExistingCategories(CategoriesArr)) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "categories" });
                }
                if (Tags != default && !await __SocialTagManagement.IsExistsTags(TagsArr)) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "tags" });
                }
                #endregion

                #region Check Permission
                var Error = __AdminUserManagement.HaveReadPermission(Session.User.Rights, ADMIN_RIGHTS.POST);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(403, RESPONSE_MESSAGES.USER_DOES_NOT_HAVE_PERMISSION, new string[]{ "see social post" });
                }
                #endregion

                #region Get posts
                List<SocialPost> Posts  = default;
                int TotalSize           = default;
                (Posts, TotalSize, Error) = await __SocialPostManagement
                    .GetPostsByAdminUser(
                        Start,
                        Size,
                        SearchTerm,
                        Owner,
                        StatusArr,
                        CombineOrders,
                        TagsArr,
                        CategoriesArr
                    );
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetPostsByAdminUser failed, ErrorCode: { Error }");
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
                    var Obj = e.GetPublicShortJsonObject(default, true);
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
