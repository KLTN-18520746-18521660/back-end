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

namespace CoreApi.Controllers.Social.Session
{
    [ApiController]
    [Route("/category")]
    public class GetCategoriesController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetCategoriesController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "GetCategories";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.ToString());
                if (Error != e.Message && Error != "") {
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
                bool isSessionInvalid = false;
                #region Get session token
                if (session_token == null) {
                    LogDebug($"Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    LogDebug("Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                if (isSessionInvalid) {
                    (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                    if (error != ErrorCodes.NO_ERROR) {
                        isSessionInvalid = false;
                    }
                }
                #endregion

                IReadOnlyList<SocialCategory> categories = null;
                (categories, error) = await __SocialCategoryManagement.GetCategories();

                LogDebug("GetCategories success.");
                return Ok(200, "Ok", new JObject(){
                    { "categories", Utils.ObjectToJsonToken(categories) },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
