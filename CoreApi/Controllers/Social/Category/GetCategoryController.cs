using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Category
{
    [ApiController]
    [Route("/api/category")]
    public class GetCategoryController : BaseController
    {
        public GetCategoryController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "GetCategory";
        }

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetCategories([FromServices] SessionSocialUserManagement   __SessionSocialUserManagement,
                                                       [FromServices] SocialCategoryManagement      __SocialCategoryManagement,
                                                       [FromHeader(Name = "session_token")] string  SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialCategoryManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
                var Session             = __Session as SessionSocialUser;
                #endregion

                var (Categories, Error) = await __SocialCategoryManagement.GetCategories();

                var Ret = new List<JObject>();
                Categories.ForEach(e => {
                    var Obj = e.GetPublicJsonObject();
                    if (IsValidSession) {
                        Obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    }
                    Ret.Add(Obj);
                });

                return Ok(200, "OK", new JObject(){
                    { "categories", Utils.ObjectToJsonToken(Ret) },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }

        [HttpGet("{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetCategoriesBySlug([FromServices] SessionSocialUserManagement     __SessionSocialUserManagement,
                                                             [FromServices] SocialCategoryManagement        __SocialCategoryManagement,
                                                             [FromRoute(Name = "category")] string          __Category,
                                                             [FromHeader(Name = "session_token")] string    SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialCategoryManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Validate params
                AddLogParam("category", __Category);
                if (!__SocialCategoryManagement.IsValidCategory(__Category)) {
                    return Problem(400, "Invalid category.");
                }
                #endregion

                var (FindCategory, Error) = await __SocialCategoryManagement.FindCategoryBySlug(__Category);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found category.");
                    }
                    throw new Exception($"FindCategoryBySlug failed, ErrorCode: { Error }");
                }

                var Ret = FindCategory.GetPublicJsonObject();
                if (IsValidSession) {
                    Ret.Add("actions", Utils.ObjectToJsonToken(FindCategory.GetActionByUser(Session.UserId)));
                }

                return Ok(200, "OK", new JObject(){
                    { "category", Utils.ObjectToJsonToken(Ret) },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
