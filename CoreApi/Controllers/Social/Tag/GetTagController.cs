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

namespace CoreApi.Controllers.Social.Tag
{
    [ApiController]
    [Route("/tag")]
    public class GetTagController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetTagController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "GetTag";
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
        public async Task<IActionResult> GetTags([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                 [FromServices] SocialTagManagement __SocialTagManagement,
                                                 [FromHeader] string session_token,
                                                 [FromQuery] int start = 0,
                                                 [FromQuery] int size = 20,
                                                 [FromQuery] string search_term = default)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialTagManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (start < 0 || size < 1) {
                    return Problem(400, "Bad request params.");
                }
                #endregion
                bool IsValidSession = true;
                #region Get session token
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

                List<SocialTag> tags = default;
                int totalSize = 0;
                (tags, totalSize) = await __SocialTagManagement
                                                    .GetTags(start, size, search_term, IsValidSession ? session.UserId : default);
                #region Validate params: start, size, total_size
                if (totalSize != 0 && start >= totalSize) {
                    LogWarning($"Invalid request params for get tags, start: { start }, size: { size }, search_term: { search_term }, total_size: { totalSize }");
                    return Problem(400, $"Invalid request params start: { start }. Total size is { totalSize }");
                }
                #endregion

                var ret = new List<JObject>();
                tags.ForEach(e => {
                    var obj = e.GetPublicJsonObject();
                    if (IsValidSession) {
                        obj.Add("actions", Utils.ObjectToJsonToken(e.GetActionWithUser(session.UserId)));
                    }
                    ret.Add(obj);
                });

                LogDebug("GetTags success.");
                return Ok(200, "Ok", new JObject(){
                    { "tags", Utils.ObjectToJsonToken(ret) },
                    { "total_size", totalSize },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }

        [HttpGet("{tag}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetTagByName([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                     [FromServices] SocialTagManagement __SocialTagManagement,
                                                     [FromRoute] string tag,
                                                     [FromHeader] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialTagManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate tag
                if (!__SocialTagManagement.IsValidTag(tag)) {
                    return Problem(400, "Invalid tag.");
                }
                #endregion
                bool IsValidSession = true;
                #region Get session token
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

                SocialTag findTag = default;
                if (IsValidSession) {
                    (findTag, error) = await __SocialTagManagement.FindTagByName(tag, session.UserId);
                } else {
                    (findTag, error) = await __SocialTagManagement.FindTagByName(tag);
                }

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        return Problem(404, "Not found tag.");
                    }
                    throw new Exception($"FindTagByName failed, ErrorCode: { error }");
                }

                var ret = findTag.GetPublicJsonObject();
                if (IsValidSession) {
                    ret.Add("actions", Utils.ObjectToJsonToken(findTag.GetActionWithUser(session.UserId)));
                }

                LogDebug("GetTagByName success.");
                return Ok(200, "Ok", new JObject(){
                    { "tag", Utils.ObjectToJsonToken(ret) },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
