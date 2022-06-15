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
using System.Linq;
using System.Globalization;

namespace CoreApi.Controllers.Admin.Statistic
{
    [ApiController]
    [Route("/api/admin/statistic/post/chart")]
    public class AdminGetPostChartController : BaseController
    {
        private static string DATE_TIME_FORMAT = "dd/MM/yyyy";
        public AdminGetPostChartController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
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
        public async Task<IActionResult> AdminGetPostChart([FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                           [FromServices] SocialCategoryManagement __SocialCategoryManagement,
                                                           [FromServices] SocialPostManagement __SocialPostManagement,
                                                           [FromServices] AdminUserManagement __AdminUserManagement,
                                                           [FromServices] SocialTagManagement __SocialTagManagement,
                                                           [FromHeader(Name = "session_token_admin")] string SessionToken,
                                                           [FromQuery(Name = "tags")] string Tags = default,
                                                           [FromQuery(Name = "categories")] string Categories = default,
                                                           [FromQuery(Name = "start")] string Start = default,
                                                           [FromQuery(Name = "end")] string End = default)
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
                AddLogParam("categories", Categories);
                AddLogParam("tags", Tags);
                AddLogParam("start", Start);
                AddLogParam("end", End);
                if (!DateTime.TryParseExact(Start, DATE_TIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var StartDate)
                    || StartDate.ToString(DATE_TIME_FORMAT) != Start) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if (!DateTime.TryParseExact(End, DATE_TIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var EndDate)
                    || EndDate.ToString(DATE_TIME_FORMAT) != End) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if (StartDate.CompareTo(EndDate) >= 0) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }

                var TagsArr                     = Tags == default ? default : Tags.Split(',');
                var CategoriesArr               = Categories == default ? default : Categories.Split(',');
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
                    return Problem(403, RESPONSE_MESSAGES.USER_DOES_NOT_HAVE_PERMISSION, new string[]{ "see post statistics" });
                }
                #endregion

                #region Get statistics
                JObject Ret     = default;
                (Ret, Error)    = await __SocialPostManagement
                    .GetChartStatistic(
                        Start,
                        End,
                        DATE_TIME_FORMAT,
                        TagsArr,
                        CategoriesArr
                    );
                if (Error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"GetChartStatistic failed, ErrorCode: { Error }");
                }
                #endregion

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "chart", Ret },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
