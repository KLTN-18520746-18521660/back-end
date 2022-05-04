using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetCategoryController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "GetCategory";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = string.Empty;
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.ToString());
                if (Error != e.Message && Error != string.Empty) {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value failed, message: { msg }");
            }
        }

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetCategories([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                       [FromServices] SocialCategoryManagement __SocialCategoryManagement,
                                                       [FromHeader] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialCategoryManagement.SetTraceId(TraceId);
            #endregion
            try {
                bool IsValidSession = true;
                #region Get session token
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                    IsValidSession = false;
                }

                if (IsValidSession && !CommonValidate.IsValidSessionToken(session_token)) {
                    LogDebug("Invalid header authorization.");
                    IsValidSession = false;
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                if (IsValidSession) {
                    (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                    if (error != ErrorCodes.NO_ERROR) {
                        IsValidSession = false;
                    }
                }
                #endregion

                List<SocialCategory> categories = default;
                (categories, error) = await __SocialCategoryManagement.GetCategories();

                var ret = new List<JObject>();
                categories.ForEach(e => {
                    var obj = e.GetPublicJsonObject();
                    if (IsValidSession) {
                        obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(session.UserId)));
                    }
                    ret.Add(obj);
                });

                LogDebug("GetCategories success.");
                return Ok(200, "OK", new JObject(){
                    { "categories", Utils.ObjectToJsonToken(ret) },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }

        [HttpGet("{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetCategoriesBySlug([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                             [FromServices] SocialCategoryManagement __SocialCategoryManagement,
                                                             [FromRoute] string category,
                                                             [FromHeader] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialCategoryManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (!__SocialCategoryManagement.IsValidCategory(category)) {
                    return Problem(400, "Invalid category.");
                }
                #endregion
                bool IsValidSession = true;
                #region Get session token
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                    IsValidSession = false;
                }

                if (IsValidSession && !CommonValidate.IsValidSessionToken(session_token)) {
                    LogDebug("Invalid header authorization.");
                    IsValidSession = false;
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                if (IsValidSession) {
                    (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                    if (error != ErrorCodes.NO_ERROR) {
                        IsValidSession = false;
                    }
                }
                #endregion

                SocialCategory findCategory = default;
                (findCategory, error) = await __SocialCategoryManagement.FindCategoryBySlug(category);
                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found category.");
                    }
                    throw new Exception($"FindCategoryBySlug failed, ErrorCode: { error }");
                }

                var ret = findCategory.GetPublicJsonObject();
                if (IsValidSession) {
                    ret.Add("actions", Utils.ObjectToJsonToken(findCategory.GetActionByUser(session.UserId)));
                }

                LogDebug("GetCategories success.");
                return Ok(200, "OK", new JObject(){
                    { "category", Utils.ObjectToJsonToken(ret) },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
